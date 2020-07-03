using NiDump;
using SharpDX;
using SharpDX.Direct3D11;
using System.IO;

namespace MiniCube
{
    using ObjectRef = System.Int32;
    using StringRef = System.Int32;

    public class Mesh
    {
        NiHeader header;
        ObjectRef triShape_ref;
        BSLightingShaderProperty shader_property = null;
        BSSkinBoneData bone_data;
        public string albedoMap_path;
        //public string normalMap_path;
        public uint SLSF1
        {
            get { return shader_property.shader_flags_1; }
        }
        public uint SLSF2
        {
            get { return shader_property.shader_flags_2; }
        }
        public uint num_bones;
        public int[] bones;

        public Buffer vb_positions, vb_uvs, vb_weights, vb_indices, ib;
        public int num_triangle_points;

        public void Dispose()
        {
            ib.Dispose();
            vb_indices.Dispose();
            vb_weights.Dispose();
            vb_uvs.Dispose();
            vb_positions.Dispose();
        }

        static void ToMatrix(NiDump.Transform t, out Matrix m)
        {
            m = Matrix.Scaling(t.scale) * (Matrix)t.rotation;
            m.M41 = t.translation.X;
            m.M42 = t.translation.Y;
            m.M43 = t.translation.Z;
            m.M44 = 1;
        }

        public Mesh(Device device, NiHeader header, ObjectRef triShape_ref)
        {
            this.header = header;
            this.triShape_ref = triShape_ref;

            BSTriShape triShape = header.GetObject<BSTriShape>(triShape_ref);

            Matrix triShape_local_m;
            ToMatrix(triShape.local, out triShape_local_m);

            //var triShape_data = header.GetObject<NiTriShapeData>(triShape.data);
            shader_property = header.GetObject<BSLightingShaderProperty>(triShape.shader_property);
            var shader_texture_set = header.GetObject<BSShaderTextureSet>(shader_property.texture_set);
            var skin_instance = header.GetObject<BSSkinInstance>(triShape.skin);
            bone_data = header.GetObject<BSSkinBoneData>(skin_instance.data);

            albedoMap_path = Path.GetFileName(shader_texture_set.textures[0]);

            num_bones = skin_instance.num_bones;
            bones = skin_instance.bones;

            {
                Vector3[] positions = new Vector3[triShape.num_vertices];
                Vector2[] uvs = new Vector2[triShape.num_vertices];
                Vector4[] bone_weights = new Vector4[triShape.num_vertices];
                uint[] bone_indices = new uint[triShape.num_vertices];

                for (int i = 0; i < triShape.num_vertices; i++)
                {
                    positions[i] = triShape.vertex_data[i].vertex;
                    //Vector3.TransformCoordinate(ref positions[i], ref triShape_local_m, out positions[i]);
                    uvs[i] = triShape.vertex_data[i].uv;
                    bone_weights[i] = new Vector4(
                        triShape.vertex_data[i].bone_weights[0],
                        triShape.vertex_data[i].bone_weights[1],
                        triShape.vertex_data[i].bone_weights[2],
                        triShape.vertex_data[i].bone_weights[3]);
                    bone_indices[i] = System.BitConverter.ToUInt32(triShape.vertex_data[i].bone_indices, 0);
                }

                this.vb_positions = Buffer.Create(device, BindFlags.VertexBuffer, positions);
                this.vb_uvs = Buffer.Create(device, BindFlags.VertexBuffer, uvs);
                this.vb_weights = Buffer.Create(device, BindFlags.VertexBuffer, bone_weights);
                this.vb_indices = Buffer.Create(device, BindFlags.VertexBuffer, bone_indices);
                this.ib = Buffer.Create(device, BindFlags.IndexBuffer, triShape.triangles);

                this.num_triangle_points = triShape.triangles.Length * 3;
            }
        }

        public string GetBoneName(int i)
        {
            ObjectRef node_ref = this.bones[i];
            NiNode node = header.GetObject<NiNode>(node_ref);
            return header.strings[node.name];
        }

        static Transform ToTransform(NiDump.Transform t)
        {
            Quaternion rotation;
            Quaternion.RotationMatrix(ref t.rotation, out rotation);
            return new Transform(t.translation, rotation, t.scale);
        }

        public Transform GetBoneLocal(int i)
        {
            //NiDump.Transform t = skin_data.transform * skin_data.bone_data[i].transform;
            //return ToTransform(t);
            NiNode node = header.GetObject<NiNode>(bones[i]);
            return ToTransform(node.local);
        }
    }
}
