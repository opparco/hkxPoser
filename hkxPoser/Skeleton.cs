using System;
using NiDump;

namespace MiniCube
{
    using ObjectRef = System.Int32;
    using StringRef = System.Int32;

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
}
