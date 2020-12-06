using System;
using System.IO;
using System.Collections.Generic;
using NiDump;
using SharpDX;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

namespace MiniCube
{
    using ObjectRef = System.Int32;
    using StringRef = System.Int32;

    public class Mesh
    {
        NiHeader header;
        ObjectRef triShape_ref;

        BSTriShape triShape;
        NiSkinInstance skin_instance;
        NiSkinData skin_data;
        NiSkinPartition skin_part;

        BSLightingShaderProperty shader_property = null;
        public uint SLSF1
        {
            get { return shader_property.shader_flags_1; }
        }
        public uint SLSF2
        {
            get { return shader_property.shader_flags_2; }
        }

        public string albedoMap_path;
        //public string normalMap_path;

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

        public Mesh(Device device, NiHeader header, ObjectRef triShape_ref, bool dynamic_p = false)
        {
            this.header = header;
            this.triShape_ref = triShape_ref;

            if (dynamic_p)
                triShape = header.GetObject<BSDynamicTriShape>(triShape_ref);
            else
                triShape = header.GetObject<BSTriShape>(triShape_ref);

            skin_instance = header.GetObject<NiSkinInstance>(triShape.skin);
            skin_data = header.GetObject<NiSkinData>(skin_instance.data);
            skin_part = header.GetObject<NiSkinPartition>(skin_instance.skin_partition);

            shader_property = header.GetObject<BSLightingShaderProperty>(triShape.shader_property);
            var shader_texture_set = header.GetObject<BSShaderTextureSet>(shader_property.texture_set);

            albedoMap_path = Path.GetFileName(shader_texture_set.textures[0]);

            num_bones = skin_instance.num_bones;
            bones = skin_instance.bones;

            //
            // skinning
            //
            NiDump.Transform[] bone_transforms = new NiDump.Transform[num_bones];

            for (int i = 0; i < num_bones; i++)
            {
                ObjectRef node_ref = this.bones[i];
                NiNode node = header.GetObject<NiNode>(node_ref);
                node.self_ref = node_ref;

                NiDump.Transform node_local = node.GetLocalTransform(skin_instance.skeleton_root);
                NiDump.Transform bone_trans = skin_data.bone_data[i].transform;
                bone_transforms[i] = node_local * bone_trans;
            }
            //

            //
            // create device resources
            //
            // triShape.num_vertices = 0
            uint num_vertices = skin_part.data_size / skin_part.vertex_size;

            Vector3[] positions = new Vector3[num_vertices];
            Vector3[] skinned_positions = new Vector3[num_vertices];
            Vector2[] uvs = new Vector2[num_vertices];
            Vector4[] bone_weights = new Vector4[num_vertices];
            uint[] bone_indices = new uint[num_vertices];

            if (dynamic_p)
            {
                BSDynamicTriShape dynamicTriShape = (BSDynamicTriShape)triShape;
                {
                    for (int i = 0; i < dynamicTriShape.vertices.Length; i++)
                    {
                        // danger!
                        positions[i] = (Vector3)dynamicTriShape.vertices[i];
                    }
                }
            }

            for (int i = 0; i < num_vertices; i++)
            {
                if (! dynamic_p)
                    positions[i] = skin_part.vertex_data[i].vertex;

                //
                // skinning
                //
                skinned_positions[i] = Vector3.Zero;
                for (int x = 0; x < 4; x++)
                {
                    int bone_idx = skin_part.vertex_data[i].bone_indices[x];
                    float weight = skin_part.vertex_data[i].bone_weights[x];
                    skinned_positions[i] += bone_transforms[bone_idx] * positions[i] * weight;
                }
                //
                uvs[i] = skin_part.vertex_data[i].uv;
                bone_weights[i] = new Vector4(
                    skin_part.vertex_data[i].bone_weights[0],
                    skin_part.vertex_data[i].bone_weights[1],
                    skin_part.vertex_data[i].bone_weights[2],
                    skin_part.vertex_data[i].bone_weights[3]);
                bone_indices[i] = System.BitConverter.ToUInt32(skin_part.vertex_data[i].bone_indices, 0);
            }

            this.vb_positions = Buffer.Create(device, BindFlags.VertexBuffer, skinned_positions);
            this.vb_uvs = Buffer.Create(device, BindFlags.VertexBuffer, uvs);
            this.vb_weights = Buffer.Create(device, BindFlags.VertexBuffer, bone_weights);
            this.vb_indices = Buffer.Create(device, BindFlags.VertexBuffer, bone_indices);

            //
            // concatenate triangles in skin_part.skin_partitions
            //
            {
                int len = 0;
                for (int part_i = 0; part_i < skin_part.num_skin_partitions; part_i++)
                {
                    ref SkinPartition part = ref skin_part.skin_partitions[part_i];
                    len += part.triangles.Length;
                }
                Triangle[] triangles = new Triangle[len];

                int off = 0;
                for (int part_i = 0; part_i < skin_part.num_skin_partitions; part_i++)
                {
                    ref SkinPartition part = ref skin_part.skin_partitions[part_i];
                    part.triangles.CopyTo(triangles, off);
                    off += part.triangles.Length;
                }
                this.ib = Buffer.Create(device, BindFlags.IndexBuffer, triangles);
                this.num_triangle_points = triangles.Length * 3;
            }
            //

        }

        public string GetBoneName(int i)
        {
            ObjectRef node_ref = this.bones[i];
            NiNode node = header.GetObject<NiNode>(node_ref);
            return header.strings[node.name];
        }
    }
}
