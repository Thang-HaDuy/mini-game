using UnityEngine;

namespace MiniGame.Core.Utils
{
    public static class ChunkMath
    {
        public static Vector2Int WorldToChunk(
            Vector3 worldPos,
            Vector3Int chunkSize)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / chunkSize.x),
                Mathf.FloorToInt(worldPos.z / chunkSize.z));
        }

        public static Vector3Int WorldToLocal(Vector3Int worldPos, Vector2Int chunk, Vector3Int chunkSize)
        {
            return new Vector3Int(
                worldPos.x - chunk.x * chunkSize.x,
                worldPos.y,
                worldPos.z - chunk.y * chunkSize.z
            );
        }

        public static Vector3 ChunkToWorld(Vector2Int chunk, Vector3Int chunkSize)
        {
            return new Vector3(
                chunk.x * chunkSize.x,
                0,
                chunk.y * chunkSize.z
            );
        }
    }
}