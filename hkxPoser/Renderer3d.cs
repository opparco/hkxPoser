using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms; // for Control
using MiniCube;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace hkxPoser
{
    public class Renderer3d
    {
        Device device;
        DeviceContext context;

        Buffer cb_wvp;
        Buffer cb_mat;
        Buffer cb_shader_flags;

        TextureLoader textureLoader;
        TextureCollection textureCollection;
        BoneMapCollection boneMapCollection;

        DepthStencilView depthView;
        RenderTargetView renderView;

        hkaSkeleton skeleton;
        List<NiFile> nifs = new List<NiFile>();

        Matrix[] bone_matrices;
        uint[] shader_flags;

        public Renderer3d()
        {
            bone_matrices = new Matrix[80];
            for (int i = 0; i < 80; i++)
                bone_matrices[i] = Matrix.Identity;

            shader_flags = new uint[4];
            for (int i = 0; i < 4; i++)
                shader_flags[i] = 0;
        }

        public void Dispose()
        {
            System.Console.WriteLine("Renderer3d.Dispose");
            if (context != null)
            {
                DiscardDeviceResources();

                textureCollection?.Dispose();
                textureLoader?.Dispose();

                cb_shader_flags?.Dispose();
                cb_mat?.Dispose();
                cb_wvp?.Dispose();

                foreach (NiFile nif in nifs)
                foreach (Mesh mesh in nif.meshes)
                    mesh.Dispose();

                context.ClearState();
                context.Flush();
                context.Dispose();
                context = null;
            }
            skeleton = null;
            device = null;
        }

        void DefineInputLayout(ShaderBytecode bytecode)
        {
            InputElement[] elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0, 1),
                new InputElement("BLENDWEIGHT", 0, Format.R32G32B32A32_Float, 0, 2),
                new InputElement("BLENDINDICES", 0, Format.R8G8B8A8_UInt, 0, 3)
            };
            using (var signature = ShaderSignature.GetInputSignature(bytecode))
            using (var layout = new InputLayout(device, signature, elements))
            {
                context.InputAssembler.InputLayout = layout;
            }
        }

        public void InitializeGraphics(Device device, hkaSkeleton skeleton)
        {
            this.device = device;
            this.skeleton = skeleton;

            string meshes_path = Path.Combine(Application.StartupPath, @"data\meshes");
            string textures_path = Path.Combine(Application.StartupPath, @"data\textures");
            string shader_path = Path.Combine(Application.StartupPath, @"shader.fx");

            foreach (string file in Directory.GetFiles(meshes_path, "*.nif"))
            {
                string path = Path.Combine(meshes_path, file);
                nifs.Add(new NiFile(device, path));
            }

            textureLoader = new TextureLoader(textures_path);
            textureCollection = new TextureCollection(device, textureLoader);
            foreach (NiFile nif in nifs)
            foreach (Mesh mesh in nif.meshes)
                textureCollection.LoadTexture(mesh.albedoMap_path);

            context = device.ImmediateContext;

            using (var bytecode = ShaderBytecode.CompileFromFile(shader_path, "VS", "vs_4_0"))
            {
                DefineInputLayout(bytecode);
                using (var vertexShader = new VertexShader(device, bytecode))
                    context.VertexShader.Set(vertexShader);
            }
            using (var bytecode = ShaderBytecode.CompileFromFile(shader_path, "PS", "ps_4_0"))
            {
                using (var pixelShader = new PixelShader(device, bytecode))
                    context.PixelShader.Set(pixelShader);
            }

            cb_wvp = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            cb_mat = new Buffer(device, Utilities.SizeOf<Matrix>() * 40, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, Utilities.SizeOf<Matrix>());
            cb_shader_flags = new Buffer(device, Utilities.SizeOf<uint>() * 4, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, Utilities.SizeOf<uint>());

            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            context.VertexShader.SetConstantBuffer(0, cb_wvp);
            context.VertexShader.SetConstantBuffer(1, cb_mat);
            context.PixelShader.SetConstantBuffer(2, cb_shader_flags);

            var rasterizerStateDescription = RasterizerStateDescription.Default();
            rasterizerStateDescription.IsFrontCounterClockwise = true;
            using (var rasterizerState = new RasterizerState(device, rasterizerStateDescription))
                context.Rasterizer.State = rasterizerState;

            var blendStateDescription = new BlendStateDescription();
            blendStateDescription.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };
            using (var blendState = new BlendState(device, blendStateDescription))
                context.OutputMerger.SetBlendState(blendState);

            // from skeleton
            // name to idx
            var nameMap = new Dictionary<string, int>();

            for (int i = 0; i < skeleton.bones.Length; i++)
            {
                nameMap[skeleton.bones[i].name] = i;
            }

            boneMapCollection = new BoneMapCollection(nameMap);
            foreach (NiFile nif in nifs)
            foreach (Mesh mesh in nif.meshes)
                boneMapCollection.SetBoneMap(mesh);
        }

        public void DiscardDeviceResources()
        {
            Utilities.Dispose(ref renderView);
            Utilities.Dispose(ref depthView);
        }

        public void CreateDeviceResources(SwapChain swapChain, ref Viewport viewport)
        {
            if (depthView == null && renderView == null)
            {
                using (var resource = new Texture2D(device, new Texture2DDescription()
                {
                    Format = Format.D32_Float_S8X24_UInt,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = viewport.Width,
                    Height = viewport.Height,
                    SampleDescription = swapChain.Description.SampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                }))
                {
                    depthView = new DepthStencilView(device, resource);
                }

                using (var resource = Texture2D.FromSwapChain<Texture2D>(swapChain, 0))
                {
                    renderView = new RenderTargetView(device, resource);
                }

                context.Rasterizer.SetViewport(viewport);
                context.OutputMerger.SetTargets(depthView, renderView);
            }
        }

        public void Update(ref Matrix wvp)
        {
            context.UpdateSubresource(ref wvp, cb_wvp);
        }

        public Color ScreenColor { get; set; }

        public void Render()
        {
            context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            context.ClearRenderTargetView(renderView, ScreenColor);

            foreach (NiFile nif in nifs)
                foreach (Mesh mesh in nif.meshes)
                {
                    int[] boneMap = boneMapCollection.GetBoneMap(mesh);

                    UpdateBoneMatrices(mesh, boneMap);
                    context.UpdateSubresource<Matrix>(bone_matrices, cb_mat);
                    shader_flags[0] = mesh.SLSF1;
                    shader_flags[1] = mesh.SLSF2;
                    context.UpdateSubresource<uint>(shader_flags, cb_shader_flags);

                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(mesh.vb_positions, Utilities.SizeOf<Vector3>(), 0));
                    context.InputAssembler.SetVertexBuffers(1, new VertexBufferBinding(mesh.vb_uvs, Utilities.SizeOf<Vector2>(), 0));
                    context.InputAssembler.SetVertexBuffers(2, new VertexBufferBinding(mesh.vb_weights, Utilities.SizeOf<Vector4>(), 0));
                    context.InputAssembler.SetVertexBuffers(3, new VertexBufferBinding(mesh.vb_indices, Utilities.SizeOf<uint>(), 0));
                    context.InputAssembler.SetIndexBuffer(mesh.ib, Format.R16_UInt, 0);

                    context.PixelShader.SetShaderResource(0, textureCollection.GetTextureViewByPath(mesh.albedoMap_path));

                    context.DrawIndexed(mesh.num_triangle_points, 0, 0);
                }

            //swapChain.Present(1, PresentFlags.None);
        }

        private void UpdateBoneMatrices(Mesh mesh, int[] boneMap)
        {
            for (int i = 0; i < mesh.bones.Length; i++)
            {
                int hkabone_idx = boneMap[i];
                if (hkabone_idx != -1)
                    UpdateBoneMatrix(hkabone_idx, mesh.GetBoneLocal(i), out bone_matrices[i]);
                else
                    bone_matrices[i] = Matrix.Zero;
            }
        }

        void UpdateBoneMatrix(int hkabone_idx, Transform t, out Matrix m)
        {
            Transform hkabone_world = skeleton.bones[hkabone_idx].GetWorldCoordinate();

            Matrix hkabone_m;
            TransformToMatrix(ref hkabone_world, out hkabone_m);

            Transform refpose_world = t;

            Matrix refpose_m;
            TransformToMatrix(ref refpose_world, out refpose_m);

            refpose_m.Invert();

            m = refpose_m * hkabone_m;
        }

        void TransformToMatrix(ref Transform t, out Matrix m)
        {
            m = Matrix.Scaling(t.scale) * Matrix.RotationQuaternion(t.rotation);
            m.M41 = t.translation.X;
            m.M42 = t.translation.Y;
            m.M43 = t.translation.Z;
            m.M44 = 1;
        }
    }
}
