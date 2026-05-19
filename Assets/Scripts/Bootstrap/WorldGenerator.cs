using MiniGame.Core.Config;
using MiniGame.Core.Context;
using MiniGame.Core.Data;
using MiniGame.Core.Generation;
using MiniGame.Core.Generation.Biomes;
using MiniGame.Core.Rendering;
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
        // shorthand: block placement tại offset (x,y,z) với blockType t
        static StructureRule.BlockPlacement P(int x, int y, int z, int t) =>
            new() { offset = new Vector3Int(x, y, z), blockType = t };

        // trunk=2(dirt), leaves=4(grass), stone=1, gravel=3
        const int TRUNK = 2, LEAF = 4, STONE = 1, GRAVEL = 3;

        Biomes = new BiomeDefinition[]
        {
            // ── Cold (temp 0.00–0.33) ─────────────────────────────────────
            new()
            {
                biomeName = "Desert",
                tempMin = 0.00f, tempMax = 0.33f, humidityMin = 0.00f, humidityMax = 0.33f,
                heightMultiplier = 0.3f, baseElevation = 62,
                topBlock = GRAVEL, subSurfaceBlock = GRAVEL, fillBlock = TRUNK, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Cactus",
                        spawnChance   = 0.02f,
                        blocks        = new[]
                        {
                            P(0,0,0, GRAVEL), P(0,1,0, GRAVEL), P(0,2,0, GRAVEL),
                        }
                    }
                }
            },

            new()
            {
                biomeName = "Tundra",
                tempMin = 0.00f, tempMax = 0.33f, humidityMin = 0.33f, humidityMax = 0.67f,
                heightMultiplier = 0.4f, baseElevation = 63,
                topBlock = GRAVEL, subSurfaceBlock = TRUNK, fillBlock = GRAVEL, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Dead Tree",
                        spawnChance   = 0.01f,
                        blocks        = new[]
                        {
                            P(0,0,0, STONE), P(0,1,0, STONE),
                            P(-1,1,0, STONE), P(1,1,0, STONE),
                        }
                    }
                }
            },

            new()
            {
                biomeName = "Snowy",
                tempMin = 0.00f, tempMax = 0.33f, humidityMin = 0.67f, humidityMax = 1.00f,
                heightMultiplier = 0.5f, baseElevation = 64,
                topBlock = STONE, subSurfaceBlock = TRUNK, fillBlock = GRAVEL, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Snow Pile",
                        spawnChance   = 0.03f,
                        blocks        = new[]
                        {
                            P(0,0,0, STONE), P(-1,0,0, STONE), P(1,0,0, STONE),
                        }
                    }
                }
            },

            // ── Temperate (temp 0.33–0.67) ────────────────────────────────
            new()
            {
                biomeName = "Savanna",
                tempMin = 0.33f, tempMax = 0.67f, humidityMin = 0.00f, humidityMax = 0.33f,
                heightMultiplier = 0.4f, baseElevation = 64,
                topBlock = LEAF, subSurfaceBlock = TRUNK, fillBlock = GRAVEL, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Acacia Tree",
                        spawnChance   = 0.03f,
                        blocks        = new[]
                        {
                            // trunk
                            P(0,0,0, TRUNK), P(0,1,0, TRUNK), P(0,2,0, TRUNK),
                            // wide flat canopy
                            P(-2,3,0, LEAF), P(-1,3,0, LEAF), P(0,3,0, LEAF), P(1,3,0, LEAF), P(2,3,0, LEAF),
                            P(-2,3,-1, LEAF), P(-1,3,-1, LEAF), P(1,3,-1, LEAF), P(2,3,-1, LEAF),
                            P(-2,3, 1, LEAF), P(-1,3, 1, LEAF), P(1,3, 1, LEAF), P(2,3, 1, LEAF),
                        }
                    }
                }
            },

            new()
            {
                biomeName = "Forest",
                tempMin = 0.33f, tempMax = 0.67f, humidityMin = 0.33f, humidityMax = 0.67f,
                heightMultiplier = 0.6f, baseElevation = 65,
                topBlock = LEAF, subSurfaceBlock = TRUNK, fillBlock = TRUNK, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Oak Tree",
                        spawnChance   = 0.06f,
                        blocks        = new[]
                        {
                            // trunk
                            P(0,0,0, TRUNK), P(0,1,0, TRUNK), P(0,2,0, TRUNK), P(0,3,0, TRUNK),
                            // leaves y+2
                            P(-1,2,0, LEAF), P(1,2,0, LEAF), P(0,2,-1, LEAF), P(0,2,1, LEAF),
                            // leaves y+3
                            P(-1,3,0, LEAF), P(1,3,0, LEAF), P(0,3,-1, LEAF), P(0,3,1, LEAF),
                            P(-1,3,-1, LEAF), P(1,3,-1, LEAF), P(-1,3,1, LEAF), P(1,3,1, LEAF),
                            // top
                            P(0,4,0, LEAF), P(-1,4,0, LEAF), P(1,4,0, LEAF), P(0,4,-1, LEAF), P(0,4,1, LEAF),
                        }
                    }
                }
            },

            new()
            {
                biomeName = "Taiga",
                tempMin = 0.33f, tempMax = 0.67f, humidityMin = 0.67f, humidityMax = 1.00f,
                heightMultiplier = 0.8f, baseElevation = 66,
                topBlock = LEAF, subSurfaceBlock = TRUNK, fillBlock = TRUNK, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Pine Tree",
                        spawnChance   = 0.05f,
                        blocks        = new[]
                        {
                            // trunk
                            P(0,0,0, TRUNK), P(0,1,0, TRUNK), P(0,2,0, TRUNK),
                            P(0,3,0, TRUNK), P(0,4,0, TRUNK),
                            // cone leaves bottom (wide)
                            P(-2,2,0, LEAF), P(2,2,0, LEAF), P(0,2,-2, LEAF), P(0,2,2, LEAF),
                            P(-1,2,-1, LEAF), P(1,2,-1, LEAF), P(-1,2,1, LEAF), P(1,2,1, LEAF),
                            // cone leaves mid
                            P(-1,3,0, LEAF), P(1,3,0, LEAF), P(0,3,-1, LEAF), P(0,3,1, LEAF),
                            // cone leaves top
                            P(-1,4,0, LEAF), P(1,4,0, LEAF), P(0,4,-1, LEAF), P(0,4,1, LEAF),
                            // tip
                            P(0,5,0, LEAF),
                        }
                    }
                }
            },

            // ── Hot (temp 0.67–1.00) ──────────────────────────────────────
            new()
            {
                biomeName = "Jungle",
                tempMin = 0.67f, tempMax = 1.00f, humidityMin = 0.00f, humidityMax = 0.33f,
                heightMultiplier = 0.7f, baseElevation = 65,
                topBlock = LEAF, subSurfaceBlock = TRUNK, fillBlock = TRUNK, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Jungle Tree",
                        spawnChance   = 0.08f,
                        blocks        = new[]
                        {
                            // tall trunk
                            P(0,0,0, TRUNK), P(0,1,0, TRUNK), P(0,2,0, TRUNK),
                            P(0,3,0, TRUNK), P(0,4,0, TRUNK), P(0,5,0, TRUNK),
                            // large canopy
                            P(-2,5,0, LEAF), P(2,5,0, LEAF), P(0,5,-2, LEAF), P(0,5,2, LEAF),
                            P(-1,5,-1, LEAF), P(1,5,-1, LEAF), P(-1,5,1, LEAF), P(1,5,1, LEAF),
                            P(-1,5,0, LEAF), P(1,5,0, LEAF), P(0,5,-1, LEAF), P(0,5,1, LEAF),
                            P(-2,6,0, LEAF), P(2,6,0, LEAF), P(0,6,-2, LEAF), P(0,6,2, LEAF),
                            P(-1,6,0, LEAF), P(1,6,0, LEAF), P(0,6,-1, LEAF), P(0,6,1, LEAF),
                            P(0,7,0, LEAF), P(-1,7,0, LEAF), P(1,7,0, LEAF), P(0,7,-1, LEAF), P(0,7,1, LEAF),
                        }
                    }
                }
            },

            new()
            {
                biomeName = "Plains",
                tempMin = 0.67f, tempMax = 1.00f, humidityMin = 0.33f, humidityMax = 0.67f,
                heightMultiplier = 0.5f, baseElevation = 64,
                topBlock = LEAF, subSurfaceBlock = TRUNK, fillBlock = GRAVEL, baseBlock = STONE,
                structures = new StructureRule[] { }  // không có structure — đồng cỏ trống
            },

            new()
            {
                biomeName = "Swamp",
                tempMin = 0.67f, tempMax = 1.00f, humidityMin = 0.67f, humidityMax = 1.00f,
                heightMultiplier = 0.2f, baseElevation = 60,
                topBlock = TRUNK, subSurfaceBlock = TRUNK, fillBlock = GRAVEL, baseBlock = STONE,
                structures = new StructureRule[]
                {
                    new()
                    {
                        structureName = "Mushroom",
                        spawnChance   = 0.04f,
                        blocks        = new[]
                        {
                            // stem
                            P(0,0,0, TRUNK),
                            // cap
                            P(-1,1,0, LEAF), P(0,1,0, LEAF), P(1,1,0, LEAF),
                            P(0,1,-1, LEAF), P(0,1,1, LEAF),
                        }
                    },
                    new()
                    {
                        structureName = "Dead Tree",
                        spawnChance   = 0.02f,
                        blocks        = new[]
                        {
                            P(0,0,0, STONE), P(0,1,0, STONE), P(0,2,0, STONE),
                            P(-1,2,0, STONE), P(1,2,0, STONE),
                        }
                    }
                }
            },
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

        data.Blocks[local.x, local.y, local.z] = blockType;

        if (Context.State.ActiveChunks.ContainsKey(chunk))
            Context.ChunkManager.RequestRebuild(chunk);
    }
}
