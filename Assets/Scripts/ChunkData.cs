using UnityEngine;

public class ChunkData
{
    public Vector2Int ChunkCoord;
    public int[,,] Blocks;

    public ChunkData(Vector2Int chunkCoord)
    {
        ChunkCoord = chunkCoord;

        Blocks = new int[
            WorldGenerator.ChunkSize.x,
            WorldGenerator.ChunkSize.y,
            WorldGenerator.ChunkSize.z
        ];
    }

    public int GetBlock(int x, int y, int z)
    {
        return Blocks[x, y, z];
    }

    public void SetBlock(int x, int y, int z, int block)
    {
        Blocks[x, y, z] = block;
    }
}