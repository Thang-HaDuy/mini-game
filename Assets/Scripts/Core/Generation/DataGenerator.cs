using System;
using System.Collections;
using System.Threading.Tasks;
using MiniGame.Core.Config;
using MiniGame.Core.Context;
using MiniGame.Core.Data;
using MiniGame.Core.Generation.Biomes;
using MiniGame.Core.Interfaces;
using UnityEngine;

namespace MiniGame.Core.Generation
{
    public class DataGenerator : IDataGenerator
    {
        private readonly WorldContext world;
        private readonly BiomeSelector biomeSelector;

        public DataGenerator(WorldContext world, BiomeSelector biomeSelector)
        {
            this.world = world;
            this.biomeSelector = biomeSelector;
        }

        public IEnumerator GenerateData(Vector2Int chunkCoord, Action<ChunkData> callback)
        {
            WorldConfig cfg = world.Config;

            ChunkData chunkData = world.Storage.GetChunk(chunkCoord)
                               ?? new ChunkData(chunkCoord, cfg.ChunkSize);

            Task task = Task.Run(() => FillBlocks(chunkData.Blocks, chunkCoord, cfg));

            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
                yield break;
            }

            world.Storage.AddChunk(chunkData);
            callback(chunkData);
        }

        private void FillBlocks(int[,,] blocks, Vector2Int chunkCoord, WorldConfig cfg)
        {
            Vector3Int size = cfg.ChunkSize;
            var rng = new System.Random(chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663);

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    int worldX = x + chunkCoord.x * size.x;
                    int worldZ = z + chunkCoord.y * size.z;

                    // Terrain noise
                    float nx = cfg.NoiseOffset.x + worldX / (float)size.x * cfg.NoiseScale.x;
                    float nz = cfg.NoiseOffset.y + worldZ / (float)size.z * cfg.NoiseScale.y;

                    // Biome noise — offset khác terrain để tránh tương quan
                    float temperature = Mathf.PerlinNoise(worldX * cfg.TemperatureScale + 500f,
                                                          worldZ * cfg.TemperatureScale + 500f);
                    float humidity    = Mathf.PerlinNoise(worldX * cfg.HumidityScale    + 1000f,
                                                          worldZ * cfg.HumidityScale    + 1000f);

                    IBiome biome = biomeSelector.Select(temperature, humidity);

                    int surfaceHeight = Mathf.RoundToInt(
                        Mathf.PerlinNoise(nx, nz) * cfg.HeightIntensity * biome.HeightMultiplier
                        + biome.BaseElevation
                    );

                    surfaceHeight = Mathf.Clamp(surfaceHeight, 1, size.y - 1);

                    for (int y = surfaceHeight; y >= 0; y--)
                        if (blocks[x, y, z] == 0)
                            blocks[x, y, z] = biome.GetBlockAt(y, surfaceHeight);

                    if (biome.Structures != null)
                        foreach (var rule in biome.Structures)
                            rule.TryApply(blocks, x, surfaceHeight, z, rng);
                }
            }
        }
    }
}
