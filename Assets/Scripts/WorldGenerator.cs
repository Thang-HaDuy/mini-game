using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    private WorldStorage worldStorage;
    public WorldStorage WorldStorage => worldStorage;

    private WorldState state;
    public WorldState State => state;

    private ChunkRenderer chunkRenderer;

    private Queue<Vector2Int> rebuildQueue = new();
    private HashSet<Vector2Int> queuedSet = new();
    private HashSet<Vector2Int> rebuilding = new();

    // public static Dictionary<Vector2Int, GameObject> ActiveChunks;
    // public static Dictionary<Vector2Int, int[,,]> AdditiveWorldData;

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
    private DataGenerator dataCreator;

    private int rebuildPerFrame = 1;

    void Start()
    {
        worldStorage = new WorldStorage();

        // ActiveChunks =
        //     new Dictionary<Vector2Int, GameObject>();

        // AdditiveWorldData =
        //     new Dictionary<Vector2Int, int[,,]>();

        meshCreator =
            new ChunkMeshCreator(
                TextureLoaderInstance,
                this
            );

        dataCreator =
            new DataGenerator(
                this,
                GetComponent<StructureGenerator>()
            );

        GetComponent<StructureGenerator>().Init(this);
        chunkRenderer = new ChunkRenderer(ChunkMaterial);

        state = new WorldState();
    }

    public IEnumerator CreateChunk(Vector2Int chunkCoord)
    {
        string chunkName =
            $"Chunk {chunkCoord.x} {chunkCoord.y}";

        GameObject newChunk =
            new GameObject(
                chunkName,
                new System.Type[]
                {
                    typeof(MeshRenderer),
                    typeof(MeshFilter),
                    typeof(MeshCollider)
                }
            );

        newChunk.transform.position =
            new Vector3(
                chunkCoord.x * ChunkSize.x,
                0f,
                chunkCoord.y * ChunkSize.z
            );

        state.ActiveChunks.Add(chunkCoord, newChunk);

        ChunkData dataToApply =
            worldStorage.GetChunk(chunkCoord);

        Mesh meshToUse = null;

        if (dataToApply == null)
        {
            dataCreator.QueueDataToGenerate(
                new DataGenerator.GenData
                {
                    GenerationPoint = chunkCoord,

                    OnComplete = chunkData =>
                    {
                        dataToApply = chunkData;
                    }
                }
            );

            yield return new WaitUntil(
                () => dataToApply != null
            );
        }

        meshCreator.QueueDataToDraw(
            new ChunkMeshCreator.CreateMesh
            {
                DataToDraw = dataToApply.Blocks,

                OnComplete = mesh =>
                {
                    meshToUse = mesh;
                }
            }
        );

        yield return new WaitUntil(
            () => meshToUse != null
        );

        if (newChunk != null)
        {
            chunkRenderer.Apply(newChunk, meshToUse);
        }
    }

    public void UpdateChunk(Vector2Int chunkCoord)
    {
        if (!state.ActiveChunks.ContainsKey(chunkCoord))
            return;

        ChunkData chunkData =
            worldStorage.GetChunk(chunkCoord);

        if (chunkData == null)
            return;

        GameObject targetChunk =
            state.ActiveChunks[chunkCoord];

        MeshFilter filter =
            targetChunk.GetComponent<MeshFilter>();

        MeshCollider collider =
            targetChunk.GetComponent<MeshCollider>();

        StartCoroutine(
            meshCreator.CreateMeshFromData(
                chunkData.Blocks,
                mesh =>
                {
                    chunkRenderer.Apply(targetChunk, mesh);
                    rebuilding.Remove(chunkCoord);
                }
            )
        );
    }

    public void SetBlock(
        Vector3Int worldPosition,
        int blockType = 0
    )
    {
        Vector2Int chunkCoords =
            ChunkCoordUtility.WorldToChunk(worldPosition);

        if (!worldStorage.HasChunk(chunkCoords))
            return;

        Vector3Int localCoords =
            ChunkCoordUtility.WorldToLocal(worldPosition, chunkCoords);

        ChunkData chunk =
            worldStorage.GetChunk(chunkCoords);

        chunk.SetBlock(
            localCoords.x,
            localCoords.y,
            localCoords.z,
            blockType
        );
        ChunkDirtyTracker.MarkDirty(chunkCoords);
    }

    private void LateUpdate()
    {
        ProcessDirtyChunks();
        ProcessRebuildQueue();
    }

    private void ProcessDirtyChunks()
    {
        while (ChunkDirtyTracker.TryConsume(out var coord))
        {
            if (queuedSet.Add(coord))
            {
                rebuildQueue.Enqueue(coord);
            }   
        }
    }
    private void ProcessRebuildQueue()
    {
        int count = rebuildPerFrame;

        while (count > 0 && rebuildQueue.Count > 0)
        {
            var coord = rebuildQueue.Dequeue();
            queuedSet.Remove(coord);

            ScheduleChunkRebuild(coord);

            count--;
        }
    }

    private void ScheduleChunkRebuild(Vector2Int coord)
    {
        if (!state.ActiveChunks.ContainsKey(coord))
            return;

        StartCoroutine(RebuildChunkRoutine(coord));
    }

    private IEnumerator RebuildChunkRoutine(Vector2Int chunkCoord)
    {
        ChunkData chunkData = worldStorage.GetChunk(chunkCoord);

        if (chunkData == null)
            yield break;

        Mesh mesh = null;

        yield return meshCreator.CreateMeshFromData(
            chunkData.Blocks,
            m => mesh = m
        );

        if (!state.ActiveChunks.TryGetValue(chunkCoord, out var chunkGO))
            yield break;

        chunkRenderer.Apply(chunkGO, mesh);
    }

}