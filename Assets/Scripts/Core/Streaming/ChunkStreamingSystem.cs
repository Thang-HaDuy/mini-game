using System.Collections.Generic;
using MiniGame.Core.Context;
using MiniGame.Core.Utils;
using UnityEngine;

namespace MiniGame.Core.Streaming
{
    public class ChunkStreamingSystem
    {
        private WorldContext world;
        private readonly int renderDistance;

        private readonly HashSet<Vector2Int> loaded = new();
        private readonly List<Vector2Int> toRemove = new();

        public ChunkStreamingSystem(WorldContext world, int renderDistance)
        {
            this.world = world;
            this.renderDistance = renderDistance;
        }

        public void Tick(Vector3 playerPos)
        {
            Vector2Int center = ChunkMath.WorldToChunk(playerPos, world.Config.ChunkSize);

            UpdateLoad(center);
            UpdateUnload(center);
        }

        private void UpdateLoad(Vector2Int center)
        {
            for (int x = center.x - renderDistance; x <= center.x + renderDistance; x++)
            {
                for (int y = center.y - renderDistance; y <= center.y + renderDistance; y++)
                {
                    var coord = new Vector2Int(x, y);

                    if (!loaded.Add(coord))
                        continue;
                    world.ChunkManager.RequestChunk(coord);
                }
            }
        }

        private void UpdateUnload(Vector2Int center)
        {
            toRemove.Clear();

            foreach (var c in loaded)
            {
                if (Mathf.Abs(c.x - center.x) > renderDistance ||
                    Mathf.Abs(c.y - center.y) > renderDistance)
                {
                    toRemove.Add(c);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                var c = toRemove[i];
                loaded.Remove(c);

                if (world.State.ActiveChunks.TryGetValue(c, out var go))
                {
                    world.State.ActiveChunks.Remove(c);
                    world.Storage.RemoveChunk(c);
                    Object.Destroy(go);
                }
            }
        }
    }
}