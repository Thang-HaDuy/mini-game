namespace MiniGame.Core.Mesh
{
    public static class ChunkMeshUtility
    {
        public static bool IsSolid(int[,,] blocks, int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
                return false;

            if (x >= blocks.GetLength(0) || y >= blocks.GetLength(1) || z >= blocks.GetLength(2))
                return false;

            return blocks[x, y, z] != 0;
        }
    }
}
