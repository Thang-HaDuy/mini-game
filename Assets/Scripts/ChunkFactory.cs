using System.Collections;
using UnityEngine;

public class ChunkFactory
{
    private readonly WorldGenerator world;

    public ChunkFactory(WorldGenerator world)
    {
        this.world = world;
    }

    public IEnumerator CreateChunk(Vector2Int chunkCoord)
    {
        if (world.State.ActiveChunks.ContainsKey(chunkCoord))
            yield break;

        GameObject chunkGO = CreateChunkObject(chunkCoord);

        ChunkData data = world.Storage.GetChunk(chunkCoord);

        if (data == null)
        {
            yield return GenerateChunkData(chunkCoord, result => data = result);
        }

        yield return GenerateMesh(data, mesh =>
        {
            world.ChunkRenderer.Apply(chunkGO, mesh);
        });
    }

    private GameObject CreateChunkObject(Vector2Int coord)
    {
        string name = $"Chunk{coord.x}{coord.y}";

        GameObject go = new GameObject(name,
            new System.Type[] { typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider) });

        go.transform.position = new Vector3(
            coord.x * WorldGenerator.ChunkSize.x,
            0f,
            coord.y * WorldGenerator.ChunkSize.z
        );

        world.State.ActiveChunks.Add(coord, go);

        return go;
    }

    private IEnumerator GenerateChunkData(Vector2Int coord, System.Action<ChunkData> callback)
    {
        bool done = false;
        ChunkData result = null;

        world.DataCreator.QueueDataToGenerate(new DataGenerator.GenData
        {
            GenerationPoint = coord,
            OnComplete = c =>
            {
                result = c;
                done = true;
            }
        });

        yield return new WaitUntil(() => done);

        callback(result);
    }

    private IEnumerator GenerateMesh(ChunkData data, System.Action<Mesh> callback)
    {
        bool done = false;
        Mesh mesh = null;

        world.MeshCreator.QueueDataToDraw(new ChunkMeshCreator.CreateMesh
        {
            DataToDraw = data.Blocks,
            OnComplete = m =>
            {
                mesh = m;
                done = true;
            }
        });

        yield return new WaitUntil(() => done);

        callback(mesh);
    }
}