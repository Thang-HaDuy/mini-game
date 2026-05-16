using System.Collections;
using UnityEngine;

public class ChunkPipeline
{
    private readonly WorldGenerator world;

    public ChunkPipeline(WorldGenerator world)
    {
        this.world = world;
    }

    public IEnumerator BuildChunk(Vector2Int coord)
    {
        // 1. CREATE OR GET DATA
        ChunkData data = world.Storage.GetChunk(coord);

        if (data == null)
        {
            yield return world.DataCreator.GenerateData(coord, d => data = d);
            world.Storage.AddChunk(data);
        }

        // 2. CREATE GAMEOBJECT IF NOT EXISTS
        if (!world.State.ActiveChunks.TryGetValue(coord, out var go))
        {
            go = CreateChunkObject(coord);
            world.State.ActiveChunks.Add(coord, go);
        }

        // 3. GENERATE MESH
        Mesh mesh = null;
        yield return world.MeshCreator.CreateMeshFromData(data.Blocks, m => mesh = m);

        // 4. APPLY
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