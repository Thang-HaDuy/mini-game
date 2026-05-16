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

        string chunkName = $"Chunk{chunkCoord.x}{chunkCoord.y}";

        GameObject newChunk = new GameObject(
            chunkName,
            new System.Type[] { typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider) }
        );

        newChunk.transform.position =
            new Vector3(chunkCoord.x * WorldGenerator.ChunkSize.x, 0f,
                        chunkCoord.y * WorldGenerator.ChunkSize.z);

        world.State.ActiveChunks.Add(chunkCoord, newChunk);

        ChunkData data = world.Storage.GetChunk(chunkCoord);

        if (data == null)
        {
            bool done = false;

            world.DataCreator.QueueDataToGenerate(new DataGenerator.GenData
            {
                GenerationPoint = chunkCoord,
                OnComplete = c => { data = c; done = true; }
            });

            yield return new WaitUntil(() => done);
        }

        Mesh mesh = null;

        world.MeshCreator.QueueDataToDraw(new ChunkMeshCreator.CreateMesh
        {
            DataToDraw = data.Blocks,
            OnComplete = m => mesh = m
        });

        yield return new WaitUntil(() => mesh != null);

        world.ChunkRenderer.Apply(newChunk, mesh);
    }
}