namespace MiniGame.Core.Mesh
{
    public static class ChunkMeshUtility
    {
        public static bool IsSolid(
            int[,,] blocks,
            int x,
            int y,
            int z)
        {
            if (x < 0 || y < 0 || z < 0)
                return false;

            if (x >= WorldGenerator.ChunkSize.x)
                return false;

            if (y >= WorldGenerator.ChunkSize.y)
                return false;

            if (z >= WorldGenerator.ChunkSize.z)
                return false;

            return blocks[x, y, z] != 0;
        }
    }
}