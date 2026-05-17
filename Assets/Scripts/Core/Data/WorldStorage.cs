using System.Collections.Generic;
using UnityEngine;
namespace MiniGame.Core.Data
{
    public class WorldStorage
    {
        private Dictionary<Vector2Int, ChunkData> chunks =
            new Dictionary<Vector2Int, ChunkData>();

        public bool HasChunk(Vector2Int coord)
        {
            return chunks.ContainsKey(coord);
        }

        public ChunkData GetChunk(Vector2Int coord)
        {
            chunks.TryGetValue(coord, out ChunkData chunk);
            return chunk;
        }

        public void AddChunk(ChunkData chunk)
        {
            chunks[chunk.ChunkCoord] = chunk;
        }

        public void RemoveChunk(Vector2Int coord)
        {
            chunks.Remove(coord);
        }
    }
}