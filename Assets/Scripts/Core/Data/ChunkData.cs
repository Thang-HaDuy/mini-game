using UnityEngine;

namespace MiniGame.Core.Data
{
    public class ChunkData
    {
        public Vector2Int ChunkCoord;
        public int[,,] Blocks;

        public ChunkData(Vector2Int chunkCoord, Vector3Int chunkSize)
        {
            ChunkCoord = chunkCoord;
            Blocks = new int[chunkSize.x, chunkSize.y, chunkSize.z];
        }
    }
}
