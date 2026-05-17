using MiniGame.Core.Config;
using MiniGame.Core.Context;
using MiniGame.Core.Data;
using MiniGame.Core.Generation;
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
    [Space]
    public Vector2 NoiseScale = Vector2.one;
    public Vector2 NoiseOffset = Vector2.zero;

    [Space]
    public int HeightOffset = 60;
    public float HeightIntensity = 5f;
    public WorldContext Context { get; private set; }

    void Start()
    {
        Context = new WorldContext();
        Context.Runtime = gameObject.AddComponent<RuntimeHost>();

        Context.Storage = new WorldStorage();
        Context.State = new WorldState();

        Context.Renderer = new ChunkRenderer(ChunkMaterial);

        Context.MeshCreator = new ChunkMeshCreator(TextureLoaderInstance, Context);
        Context.StructureGenerator = GetComponent<StructureGenerator>();

        Context.DataGenerator = new DataGenerator(Context, Context.StructureGenerator);

        Context.ChunkPipeline = new ChunkPipeline(Context);
        Context.ChunkManager = new ChunkManager(Context);
        Context.StreamingSystem = new ChunkStreamingSystem(Context, 2);

        Context.StructureGenerator.Init(Context);

        Context.Config = new WorldConfig
        {
            NoiseOffset = NoiseOffset,
            NoiseScale = NoiseScale,
            HeightIntensity = HeightIntensity,
            HeightOffset = HeightOffset,
            ChunkSize = ChunkSize
        };
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

        if (!Context.State.ActiveChunks.ContainsKey(chunk))
            return;

        Context.ChunkManager.RequestRebuild(chunk);
    }
}