using System;
using UnityEngine;

namespace MiniGame.Core.Generation.Biomes
{
    [Serializable]
    public class BiomeDefinition : IBiome
    {
        public string biomeName;

        [Header("Noise Range")]
        [Range(0f, 1f)] public float tempMin;
        [Range(0f, 1f)] public float tempMax;
        [Range(0f, 1f)] public float humidityMin;
        [Range(0f, 1f)] public float humidityMax;

        [Header("Terrain Shape")]
        public float heightMultiplier = 1f;
        public float baseElevation = 64f;

        [Header("Block Layers")]
        public int topBlock;
        public int subSurfaceBlock;
        public int fillBlock;
        public int baseBlock;

        [Header("Structures")]
        public StructureRule[] structures;

        public float HeightMultiplier => heightMultiplier;
        public float BaseElevation => baseElevation;
        public IStructureRule[] Structures => structures;

        public bool Matches(float temperature, float humidity) =>
            temperature >= tempMin && temperature < tempMax &&
            humidity >= humidityMin && humidity < humidityMax;

        public int GetBlockAt(int y, int surfaceHeight)
        {
            if (y == surfaceHeight) return topBlock;
            if (y > surfaceHeight - 4) return subSurfaceBlock;
            if (y > 0) return fillBlock;
            return baseBlock;
        }
    }
}
