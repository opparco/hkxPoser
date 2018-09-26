using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;
using NiDump;

namespace MiniCube
{
    public class NiFile
    {
        public NiHeader header;
        public Mesh[] meshes;

        public NiFile(Device device, string path)
        {
            Console.WriteLine("NiFile.ctor path:{0}", path);
            this.header = NiHeader.Load(path);

            int bt_NiTriShape = header.GetBlockTypeIdxByName("NiTriShape");
            int num_blocks = header.blocks.Length;
            List<Mesh> mesh_collection = new List<Mesh>();
            for (int i = 0; i < header.blocks.Length; i++)
            {
                if (header.blocks[i].type == bt_NiTriShape)
                {
                    Mesh mesh = new Mesh(device, header, i);
                    mesh_collection.Add(mesh);
                }
            }
            this.meshes = mesh_collection.ToArray();
        }
    }
}
