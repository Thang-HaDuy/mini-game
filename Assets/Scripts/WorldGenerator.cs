using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    private WorldStorage worldStorage;

    public WorldStorage WorldStorage => worldStorage;

    public static Dictionary<Vector2Int, GameObject> ActiveChunks;
    public static Dictionary<Vector2Int, int[,,]> AdditiveWorldData;

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

    void Start()
    {
        worldStorage = new WorldStorage();

        ActiveChunks =
            new Dictionary<Vector2Int, GameObject>();

        AdditiveWorldData =
            new Dictionary<Vector2Int, int[,,]>();

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

        ActiveChunks.Add(chunkCoord, newChunk);

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
            MeshRenderer renderer =
                newChunk.GetComponent<MeshRenderer>();

            MeshFilter filter =
                newChunk.GetComponent<MeshFilter>();

            MeshCollider collider =
                newChunk.GetComponent<MeshCollider>();

            filter.mesh = meshToUse;

            renderer.material = ChunkMaterial;

            collider.sharedMesh = meshToUse;
        }
    }

    public void UpdateChunk(Vector2Int chunkCoord)
    {
        if (!ActiveChunks.ContainsKey(chunkCoord))
            return;

        ChunkData chunkData =
            worldStorage.GetChunk(chunkCoord);

        if (chunkData == null)
            return;

        GameObject targetChunk =
            ActiveChunks[chunkCoord];

        MeshFilter filter =
            targetChunk.GetComponent<MeshFilter>();

        MeshCollider collider =
            targetChunk.GetComponent<MeshCollider>();

        StartCoroutine(
            meshCreator.CreateMeshFromData(
                chunkData.Blocks,
                mesh =>
                {
                    filter.mesh = mesh;
                    collider.sharedMesh = mesh;
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
            GetChunkCoordsFromPosition(worldPosition);

        if (!worldStorage.HasChunk(chunkCoords))
            return;

        Vector3Int localCoords =
            WorldToLocalCoords(
                worldPosition,
                chunkCoords
            );

        ChunkData chunk =
            worldStorage.GetChunk(chunkCoords);

        chunk.SetBlock(
            localCoords.x,
            localCoords.y,
            localCoords.z,
            blockType
        );

        UpdateChunk(chunkCoords);
    }

    public Vector2Int GetChunkCoordsFromPosition(
        Vector3 worldPosition
    )
    {
        return new Vector2Int(
            Mathf.FloorToInt(
                worldPosition.x / ChunkSize.x
            ),

            Mathf.FloorToInt(
                worldPosition.z / ChunkSize.z
            )
        );
    }

    public Vector3Int WorldToLocalCoords(
        Vector3Int worldPosition,
        Vector2Int chunkCoords
    )
    {
        return new Vector3Int(
            worldPosition.x -
            (chunkCoords.x * ChunkSize.x),

            worldPosition.y,

            worldPosition.z -
            (chunkCoords.y * ChunkSize.z)
        );
    }
}