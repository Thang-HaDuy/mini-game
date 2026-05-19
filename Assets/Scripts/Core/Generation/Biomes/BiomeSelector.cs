using UnityEngine;

namespace MiniGame.Core.Generation.Biomes
{
    public class BiomeSelector
    {
        private readonly IBiome[] biomes;
        private readonly IBiome fallback;

        public BiomeSelector(IBiome[] biomes)
        {
            this.biomes = biomes;
            fallback = biomes.Length > 0 ? biomes[0] : null;
        }

        public IBiome Select(float temperature, float humidity)
        {
            foreach (var biome in biomes)
                if (biome.Matches(temperature, humidity))
                    return biome;

            if (fallback == null)
                Debug.LogWarning($"No biome matched temp={temperature:F2} humidity={humidity:F2}, check biome ranges cover [0,1]x[0,1].");

            return fallback;
        }
    }
}
