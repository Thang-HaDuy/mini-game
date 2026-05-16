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
    public ChunkRenderer ChunkRenderer => chunkRenderer;

    public ChunkManager chunkManager;

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
        worldStorage = new WorldStorage();
        state = new WorldState();


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

        chunkManager = new ChunkManager(this, rebuildPerFrame);
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

    public void SetBlock(Vector3Int worldPosition, int blockType = 0)
    {
        Vector2Int chunkCoords =
            ChunkCoordUtility.WorldToChunk(worldPosition);

        if (!worldStorage.HasChunk(chunkCoords))
            return;

        Vector3Int localCoords =
            ChunkCoordUtility.WorldToLocal(worldPosition, chunkCoords);

        var chunk = worldStorage.GetChunk(chunkCoords);

        chunk.SetBlock(localCoords.x, localCoords.y, localCoords.z, blockType);

        ChunkDirtyTracker.MarkDirty(chunkCoords);
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
            chunkManager.EnqueueRebuild(coord);
        }
    }

}