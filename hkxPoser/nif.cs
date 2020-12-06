using System;
using System.Collections.Generic;
using NiDump;
using SharpDX;
using SharpDX.Direct3D11;

namespace MiniCube
{
    using ObjectRef = System.Int32;
    using StringRef = System.Int32;

    public class NiFile
    {
        public NiHeader header;
        public Mesh[] meshes;

        public NiFile(Device device, string path)
        {
            Console.WriteLine("NiFile.ctor path:{0}", path);
            this.header = NiHeader.Load(path);
            NiObject.user_version = header.user_version;
            NiObject.user_version_2 = header.user_version_2;

            int bt_BSTriShape = header.GetBlockTypeIdxByName("BSTriShape");
            int bt_BSDynamicTriShape = header.GetBlockTypeIdxByName("BSDynamicTriShape");
            // fo4:
            //int bt_BSSubIndexTriShape = header.GetBlockTypeIdxByName("BSSubIndexTriShape");
            int num_blocks = header.blocks.Length;
            List<Mesh> mesh_collection = new List<Mesh>();
            for (int i = 0; i < header.blocks.Length; i++)
            {
                if (header.blocks[i].type == bt_BSTriShape || header.blocks[i].type == bt_BSDynamicTriShape)
                {
                    Mesh mesh = new Mesh(device, header, i, header.blocks[i].type == bt_BSDynamicTriShape);
                    mesh_collection.Add(mesh);
                }
            }
            this.meshes = mesh_collection.ToArray();
        }
    }

    public class Node
    {
        NiHeader header;
        public ObjectRef node_ref;

        public StringRef name_ref = -1;
        public string name;

        public ObjectRef[] children_ref;
        public List<Node> children = new List<Node>();
        public Node parent = null;

        public Transform local;
        public Transform rest_world;

        public Node(NiHeader header, ObjectRef node_ref)
        {
            this.header = header;
            this.node_ref = node_ref;

            NiNode node = header.GetObject<NiNode>(node_ref);
            this.name_ref = node.name;
            this.name = header.strings[name_ref];
            this.children_ref = node.children;

            // local
            //
            this.local = new Transform();
            local.translation = node.local.translation;

            // gl Matrix3x3 to dx Quaternion
            ref Matrix3x3 gl_rotation = ref node.local.rotation;
            //Matrix3x3 dx_rotation;
            //Matrix3x3.Transpose(ref gl_rotation, out dx_rotation);
            Quaternion.RotationMatrix(ref gl_rotation, out local.rotation);

            local.scale = node.local.scale;
        }

        public Transform GetWorldCoordinate()
        {
            Transform t = new Transform();
            Node node = this;
            while (node != null)
            {
                t = node.local * t;
                node = node.parent;
            }
            return t;
        }
    }
}
