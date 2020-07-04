using System;
using System.Collections.Generic;
using NiDump;
using SharpDX;
using SharpDX.Direct3D11;

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
}
