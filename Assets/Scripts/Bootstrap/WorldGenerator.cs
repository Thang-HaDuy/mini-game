using MiniGame.Core.Config;
using MiniGame.Core.Context;
using MiniGame.Core.Data;
using MiniGame.Core.Generation;
using MiniGame.Core.Generation.Biomes;
using MiniGame.Core.Mesh;
using MiniGame.Core.Pipeline;
using MiniGame.Core.States;
using MiniGame.Core.Streaming;
using MiniGame.Core.Utils;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private TextureLoader TextureLoaderInstance;
    [SerializeField] private Material ChunkMaterial;

    public static readonly Vector3Int ChunkSize = new Vector3Int(16, 256, 16);

    [Header("Terrain Noise")]
    public Vector2 NoiseScale = Vector2.one;
    public Vector2 NoiseOffset = Vector2.zero;
    public float HeightIntensity = 30f;

    [Header("Biome Noise")]
    public float TemperatureScale = 0.003f;
    public float HumidityScale    = 0.003f;

    [Header("Biomes")]
    [SerializeField] private BiomeDefinition[] Biomes;

    public WorldContext Context { get; private set; }

   [ContextMenu("Reset to Default Biomes")]
    private void ResetToDefaultBiomes()
    {
        Biomes = new BiomeDefinition[]
        {
            // ── Cold (temp 0.00–0.33) ──────────────────────────────────────
            new() { biomeName="Desert",  tempMin=0.00f, tempMax=0.33f, humidityMin=0.00f, humidityMax=0.33f, heightMultiplier=0.3f, baseElevation=62, topBlock=3, subSurfaceBlock=3, fillBlock=2, baseBlock=1 },
            new() { biomeName="Tundra",  tempMin=0.00f, tempMax=0.33f, humidityMin=0.33f, humidityMax=0.67f, heightMultiplier=0.4f, baseElevation=63, topBlock=3, subSurfaceBlock=2, fillBlock=3, baseBlock=1 },
            new() { biomeName="Snowy",   tempMin=0.00f, tempMax=0.33f, humidityMin=0.67f, humidityMax=1.00f, heightMultiplier=0.5f, baseElevation=64, topBlock=1, subSurfaceBlock=2, fillBlock=3, baseBlock=1 },

            // ── Temperate (temp 0.33–0.67) ────────────────────────────────
            new() { biomeName="Savanna", tempMin=0.33f, tempMax=0.67f, humidityMin=0.00f, humidityMax=0.33f, heightMultiplier=0.4f, baseElevation=64, topBlock=4, subSurfaceBlock=2, fillBlock=3, baseBlock=1 },
            new() { biomeName="Forest",  tempMin=0.33f, tempMax=0.67f, humidityMin=0.33f, humidityMax=0.67f, heightMultiplier=0.6f, baseElevation=65, topBlock=4, subSurfaceBlock=2, fillBlock=2, baseBlock=1 },
            new() { biomeName="Taiga",   tempMin=0.33f, tempMax=0.67f, humidityMin=0.67f, humidityMax=1.00f, heightMultiplier=0.8f, baseElevation=66, topBlock=4, subSurfaceBlock=2, fillBlock=2, baseBlock=1 },

            // ── Hot (temp 0.67–1.00) ──────────────────────────────────────
            new() { biomeName="Jungle",  tempMin=0.67f, tempMax=1.00f, humidityMin=0.00f, humidityMax=0.33f, heightMultiplier=0.7f, baseElevation=65, topBlock=4, subSurfaceBlock=2, fillBlock=2, baseBlock=1 },
            new() { biomeName="Plains",  tempMin=0.67f, tempMax=1.00f, humidityMin=0.33f, humidityMax=0.67f, heightMultiplier=0.5f, baseElevation=64, topBlock=4, subSurfaceBlock=2, fillBlock=3, baseBlock=1 },
            new() { biomeName="Swamp",   tempMin=0.67f, tempMax=1.00f, humidityMin=0.67f, humidityMax=1.00f, heightMultiplier=0.2f, baseElevation=60, topBlock=2, subSurfaceBlock=2, fillBlock=3, baseBlock=1 },
        };

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
    
    void Start()
    {
        Context = new WorldContext();
        Context.Runtime = gameObject.AddComponent<RuntimeHost>();

        Context.Config = new WorldConfig
        {
            NoiseOffset      = NoiseOffset,
            NoiseScale       = NoiseScale,
            HeightIntensity  = HeightIntensity,
            ChunkSize        = ChunkSize,
            TemperatureScale = TemperatureScale,
            HumidityScale    = HumidityScale,
        };

        Context.Storage  = new WorldStorage();
        Context.State    = new WorldState();
        Context.Renderer = new ChunkRenderer(ChunkMaterial);
        Context.MeshCreator = new ChunkMeshCreator(TextureLoaderInstance);

        var biomeSelector = new BiomeSelector(Biomes);
        Context.DataGenerator = new DataGenerator(Context, biomeSelector);

        Context.ChunkPipeline    = new ChunkPipeline(Context);
        Context.ChunkManager     = new ChunkManager(Context);
        Context.StreamingSystem  = new ChunkStreamingSystem(Context, 2);
    }

    void LateUpdate()
    {
        Context.StreamingSystem.Tick(Player.position);
        Context.ChunkManager.ProcessQueue();
    }

    public void SetBlock(Vector3Int worldPosition, int blockType)
    {
        var chunk = ChunkMath.WorldToChunk(worldPosition, Context.Config.ChunkSize);
        if (!Context.Storage.HasChunk(chunk)) return;

        var local = ChunkMath.WorldToLocal(worldPosition, chunk, Context.Config.ChunkSize);
        var data = Context.Storage.GetChunk(chunk);
        if (data == null) return;

        data.SetBlock(local.x, local.y, local.z, blockType);

        if (Context.State.ActiveChunks.ContainsKey(chunk))
            Context.ChunkManager.RequestRebuild(chunk);
    }
}
