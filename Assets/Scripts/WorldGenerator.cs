using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    private WorldState state;
    private WorldStorage storage;
    private ChunkManager chunkManager;

    public WorldState State => state;
    public WorldStorage Storage => storage;
    public ChunkManager ChunkManager => chunkManager;

    private ChunkRenderer chunkRenderer;
    public ChunkRenderer ChunkRenderer => chunkRenderer;


    public static readonly Vector3Int ChunkSize =
        new Vector3Int(16, 256, 16);

    [SerializeField] private TextureLoader TextureLoaderInstance;
    [SerializeField] private Material ChunkMaterial;

    [Space]
    public Vector2 NoiseScale = Vector2.one;
    public Vector2 NoiseOffset = Vector2.zero;

    [Space]
    public int HeightOffset = 60;
    public float HeightIntensity = 5f;

    private ChunkMeshCreator meshCreator;
    public ChunkMeshCreator MeshCreator => meshCreator;
    private DataGenerator dataCreator;

    private int rebuildPerFrame = 1;

    void Start()
    {
        storage = new WorldStorage();
        state = new WorldState();

        meshCreator = new ChunkMeshCreator(TextureLoaderInstance, this);
        dataCreator = new DataGenerator(this, GetComponent<StructureGenerator>());

        GetComponent<StructureGenerator>().Init(this);

        chunkRenderer = new ChunkRenderer(ChunkMaterial);

        chunkManager = new ChunkManager(this, rebuildPerFrame);
    }

    public IEnumerator CreateChunk(Vector2Int chunkCoord)
    {
        if (state.ActiveChunks.ContainsKey(chunkCoord))
            yield break;
        string chunkName = $"Chunk{chunkCoord.x}{chunkCoord.y}";

        GameObject newChunk =
            new GameObject(chunkName, new System.Type[]
            {
                typeof(MeshRenderer),
                typeof(MeshFilter),
                typeof(MeshCollider)
            });

        newChunk.transform.position =
            new Vector3(chunkCoord.x * ChunkSize.x, 0f, chunkCoord.y * ChunkSize.z);

        state.ActiveChunks.Add(chunkCoord, newChunk);

        // 1. get data
        ChunkData dataToApply = storage.GetChunk(chunkCoord);

        if (dataToApply == null)
        {
            bool done = false;

            dataCreator.QueueDataToGenerate(new DataGenerator.GenData
            {
                GenerationPoint = chunkCoord,
                OnComplete = c =>
                {
                    dataToApply = c;
                    done = true;
                }
            });

            yield return new WaitUntil(() => done);
        }

        // 2. mesh
        Mesh mesh = null;

        meshCreator.QueueDataToDraw(new ChunkMeshCreator.CreateMesh
        {
            DataToDraw = dataToApply.Blocks,
            OnComplete = m => mesh = m
        });

        yield return new WaitUntil(() => mesh != null);

        // 3. apply
        chunkRenderer.Apply(newChunk, mesh);
    }
    public void SetBlock(Vector3Int worldPosition, int blockType = 0)
    {
        Vector2Int chunkCoords = ChunkCoordUtility.WorldToChunk(worldPosition);

        if (!storage.HasChunk(chunkCoords))
            return;

        Vector3Int local = ChunkCoordUtility.WorldToLocal(worldPosition, chunkCoords);

        var chunk = storage.GetChunk(chunkCoords);
        if (chunk == null) return;

        chunk.SetBlock(local.x, local.y, local.z, blockType);

        if (!state.ActiveChunks.ContainsKey(chunkCoords))
            return;

        chunkManager.RequestRebuild(chunkCoords);
    }

    private void LateUpdate()
    {
        ProcessDirtyChunks();
        chunkManager.ProcessQueue();
    }

    private void ProcessDirtyChunks()
    {
        while (ChunkDirtyTracker.TryConsume(out var coord))
        {
            chunkManager.RequestRebuild(coord);
        }
    }

}