using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkPipeline
{
    private readonly WorldGenerator world;
    private readonly Queue<Vector2Int> queue = new();
    private bool isRunning;

    public ChunkPipeline(WorldGenerator world)
    {
        this.world = world;
    }

    public IEnumerator BuildChunk(Vector2Int coord)
    {
        ChunkData data = world.Storage.GetChunk(coord);

        if (data == null)
        {
            yield return world.DataCreator.GenerateData(coord, d => data = d);

            if (data == null)
                yield break;

            world.Storage.AddChunk(data);
        }

        if (!world.State.ActiveChunks.TryGetValue(coord, out var go))
        {
            go = CreateChunkObject(coord);
            world.State.ActiveChunks[coord] = go;
        }

        Mesh mesh = null;
        yield return world.MeshCreator.CreateMeshFromData(data.Blocks, m => mesh = m);

        world.ChunkRenderer.Apply(go, mesh);
    }

    private GameObject CreateChunkObject(Vector2Int coord)
    {
        var go = new GameObject($"Chunk_{coord.x}_{coord.y}");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.AddComponent<MeshCollider>();

        go.transform.position = new Vector3(
            coord.x * WorldGenerator.ChunkSize.x,
            0,
            coord.y * WorldGenerator.ChunkSize.z
        );

        return go;
    }
}