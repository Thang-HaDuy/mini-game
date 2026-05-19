using System;
using UnityEngine;

namespace MiniGame.Core.Generation.Biomes
{
    [Serializable]
    public class StructureRule : IStructureRule
    {
        [Serializable]
        public class BlockPlacement
        {
            public Vector3Int offset;
            public int blockType;
        }

        public string structureName;
        public BlockPlacement[] blocks;
        [Range(0f, 1f)] public float spawnChance;

        public void TryApply(int[,,] data, int x, int surfaceY, int z, System.Random rng)
        {
            if (rng.NextDouble() > spawnChance)
                return;

            int sizeX = data.GetLength(0);
            int sizeY = data.GetLength(1);
            int sizeZ = data.GetLength(2);

            foreach (var b in blocks)
            {
                int px = x + b.offset.x;
                int py = surfaceY + 1 + b.offset.y;
                int pz = z + b.offset.z;

                if (px >= 0 && px < sizeX && py >= 0 && py < sizeY && pz >= 0 && pz < sizeZ)
                    data[px, py, pz] = b.blockType;
            }
        }
    }
}
