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

        Context.Storage = new WorldStorage();
        Context.State = new WorldState();

        Context.Renderer = new ChunkRenderer(ChunkMaterial);

        Context.MeshCreator = new ChunkMeshCreator(TextureLoaderInstance, this);
        Context.StructureGenerator = GetComponent<StructureGenerator>();

        Context.DataGenerator = new DataGenerator(this, Context.StructureGenerator);

        Context.ChunkPipeline = new ChunkPipeline(this);
        Context.ChunkManager = new ChunkManager(this);
        Context.StreamingSystem = new ChunkStreamingSystem(this, 2);

        Context.StructureGenerator.Init(this);

        Context.Config = new WorldConfig
        {
            NoiseOffset = NoiseOffset,
            NoiseScale = NoiseScale,
            HeightIntensity = HeightIntensity,
            HeightOffset = HeightOffset
        };
    }

    void LateUpdate()
    {
        Context.StreamingSystem.Tick(Player.position);
        Context.ChunkManager.ProcessQueue();
    }

    public void SetBlock(Vector3Int worldPosition, int blockType)
    {
        var chunk = ChunkMath.WorldToChunk(worldPosition);
        if (!Context.Storage.HasChunk(chunk)) return;

        var local = ChunkMath.WorldToLocal(worldPosition, chunk);
        var data = Context.Storage.GetChunk(chunk);
        if (data == null) return;

        data.SetBlock(local.x, local.y, local.z, blockType);

        if (!Context.State.ActiveChunks.ContainsKey(chunk))
            return;

        Context.ChunkManager.RequestRebuild(chunk);
    }
}