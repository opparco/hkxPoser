using NiDump;
using SharpDX;
using SharpDX.Direct3D11;
using System.IO;

namespace MiniCube
{
    using ObjectRef = System.Int32;
    using StringRef = System.Int32;

    public class SubMesh
    {
        public Buffer vb_positions, vb_uvs, vb_weights, vb_indices, ib;
        public int num_triangle_points;
        public ushort[] bones;

        public void Dispose()
        {
            ib.Dispose();
            vb_indices.Dispose();
            vb_weights.Dispose();
            vb_uvs.Dispose();
            vb_positions.Dispose();
        }

        public SubMesh(Device device, Vector3[] positions, Vector2[] uvs, Vector4[] weights, uint[] indices, Triangle[] triangles, ushort[] bones)
        {
            this.vb_positions = Buffer.Create(device, BindFlags.VertexBuffer, positions);
            this.vb_uvs = Buffer.Create(device, BindFlags.VertexBuffer, uvs);
            this.vb_weights = Buffer.Create(device, BindFlags.VertexBuffer, weights);
            this.vb_indices = Buffer.Create(device, BindFlags.VertexBuffer, indices);
            this.ib = Buffer.Create(device, BindFlags.IndexBuffer, triangles);

            this.num_triangle_points = triangles.Length * 3;
            this.bones = bones;
        }
    }

    public class Mesh
    {
        NiHeader header;
        ObjectRef triShape_ref;
        BSLightingShaderProperty shader_property = null;
        NiSkinData skin_data;
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
        public SubMesh[] submeshes;

        public void Dispose()
        {
            foreach (SubMesh submesh in this.submeshes)
                submesh.Dispose();
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

            NiTriShape triShape = header.GetObject<NiTriShape>(triShape_ref);

            Matrix triShape_local_m;
            ToMatrix(triShape.local, out triShape_local_m);

            var triShape_data = header.GetObject<NiTriShapeData>(triShape.data);
            shader_property = header.GetObject<BSLightingShaderProperty>(triShape.shader_property);
            var shader_texture_set = header.GetObject<BSShaderTextureSet>(shader_property.texture_set);
            var skin_instance = header.GetObject<NiSkinInstance>(triShape.skin_instance);
            skin_data = header.GetObject<NiSkinData>(skin_instance.data);
            var skin_partition = header.GetObject<NiSkinPartition>(skin_instance.skin_partition);

            albedoMap_path = Path.GetFileName(shader_texture_set.textures[0]);

            num_bones = skin_instance.num_bones;
            bones = skin_instance.bones;

            submeshes = new SubMesh[skin_partition.num_skin_partitions];
            for (int part_i = 0; part_i < skin_partition.num_skin_partitions; part_i++)
            {
                ref SkinPartition part = ref skin_partition.skin_partitions[part_i];

                // create submesh vertices/uvs from part.vertex_map

                Vector3[] positions = new Vector3[part.num_vertices];
                Vector2[] uvs = new Vector2[part.num_vertices];

                for (int i = 0; i < part.num_vertices; i++)
                {
                    ushort x = part.vertex_map[i];
                    Vector3.TransformCoordinate(ref triShape_data.vertices[x], ref triShape_local_m, out positions[i]);
                    uvs[i] = triShape_data.uvs[x];
                }

                submeshes[part_i] = new SubMesh(device, positions, uvs, part.vertex_weights, part.bone_indices, part.triangles, part.bones);
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
