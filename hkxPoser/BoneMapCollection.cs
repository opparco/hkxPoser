using System.Collections.Generic;

namespace MiniCube
{
    public class BoneMapCollection
    {
        Dictionary<Mesh, int[]> meshMap = new Dictionary<Mesh, int[]>();
        Dictionary<string, int> nameMap;

        public BoneMapCollection(Dictionary<string, int> nameMap)
        {
            this.nameMap = nameMap;
        }

        public void SetBoneMap(Mesh mesh)
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

        public int[] GetBoneMap(Mesh mesh)
        {
            return meshMap[mesh];
        }
    }
}
