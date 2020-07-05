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
            int bt_BSSubIndexTriShape = header.GetBlockTypeIdxByName("BSSubIndexTriShape");
            int num_blocks = header.blocks.Length;
            List<Mesh> mesh_collection = new List<Mesh>();
            for (int i = 0; i < header.blocks.Length; i++)
            {
                if (header.blocks[i].type == bt_BSSubIndexTriShape)
                {
                    Mesh mesh = new Mesh(device, header, i);
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

    public class Skeleton
    {
        public NiHeader header;
        public Node[] nodes;
        ObjectRef root_ref = -1;

        public Skeleton(string path)
        {
            Console.WriteLine("Skeleton.ctor path:{0}", path);
            this.header = NiHeader.Load(path);
            NiObject.user_version = header.user_version;
            NiObject.user_version_2 = header.user_version_2;

            int bt_NiNode = header.GetBlockTypeIdxByName("NiNode");
            StringRef root_name_ref = header.GetStringRefByName("Root");

            nodes = new Node[header.blocks.Length];
            for (int i = 0; i < header.blocks.Length; i++)
            {
                if (header.blocks[i].type == bt_NiNode)
                {
                    Node node = new Node(header, i);
                    nodes[i] = node;

                    if (node.name_ref == root_name_ref)
                        root_ref = i;
                }
            }

            SetNodeParent(nodes[root_ref]);

            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];

                if (node == null)
                    continue;

                node.rest_world = node.GetWorldCoordinate();
            }
        }

        public void SetNodeParent(Node root)
        {
            foreach(ObjectRef node_ref in root.children_ref)
            {
                Node node = nodes[node_ref];
                node.parent = root;
                root.children.Add(node);

                SetNodeParent(node);
            }
        }
    }

    public class NodeMapCollection
    {
        Dictionary<Mesh, int[]> meshMap = new Dictionary<Mesh, int[]>();
        Dictionary<string, int> nameMap;

        public NodeMapCollection(Dictionary<string, int> nameMap)
        {
            this.nameMap = nameMap;
        }

        public void SetNodeMap(Mesh mesh)
        {
            int[] boneMap = new int[mesh.num_bones];

            for (ushort i = 0; i < mesh.num_bones; i++)
            {
                string name = mesh.GetBoneName(i);

                int idx;
                if (nameMap.TryGetValue(name, out idx))
                {
                    boneMap[i] = idx;
                }
                else
                    boneMap[i] = -1;
            }
            meshMap[mesh] = boneMap;
        }

        public int[] GetNodeMap(Mesh mesh)
        {
            return meshMap[mesh];
        }
    }
}
