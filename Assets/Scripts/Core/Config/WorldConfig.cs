using UnityEngine;

namespace MiniGame.Core.Config
{
    public class WorldConfig
    {
        public Vector2 NoiseOffset;
        public Vector2 NoiseScale;
        public float HeightIntensity;
        public Vector3Int ChunkSize;

        // Scales for independent biome noise axes
        public float TemperatureScale;
        public float HumidityScale;
    }
}
