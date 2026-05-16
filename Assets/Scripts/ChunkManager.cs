// FILE: ChunkManager.cs (AFTER FIX)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
    private readonly WorldGenerator worldGen;
    private readonly WorldContext world;

    private readonly Queue<Vector2Int> rebuildQueue = new();
    private readonly HashSet<Vector2Int> queuedSet = new();
    private readonly int rebuildPerFrame;

    public ChunkManager(WorldGenerator worldGen, int rebuildPerFrame = 1)
    {
        this.worldGen = worldGen;
        this.world = worldGen.Context;
        this.rebuildPerFrame = rebuildPerFrame;
    }

    public void RequestChunk(Vector2Int coord)
    {
        if (world.State.ActiveChunks.ContainsKey(coord))
            return;

        worldGen.StartCoroutine(world.ChunkPipeline.BuildChunk(coord));
    }

    public void RequestRebuild(Vector2Int coord)
    {
        if (queuedSet.Add(coord))
            rebuildQueue.Enqueue(coord);
    }

    public void ProcessQueue()
    {
        int count = rebuildPerFrame;

        while (count > 0 && rebuildQueue.Count > 0)
        {
            var coord = rebuildQueue.Dequeue();
            queuedSet.Remove(coord);

            worldGen.StartCoroutine(RebuildChunkRoutine(coord));
            count--;
        }
    }

    private IEnumerator RebuildChunkRoutine(Vector2Int chunkCoord)
    {
        if (!world.State.ActiveChunks.ContainsKey(chunkCoord))
            yield break;

        var chunkData = world.Storage.GetChunk(chunkCoord);
        if (chunkData == null)
            yield break;

        Mesh mesh = null;

        yield return world.MeshCreator.CreateMeshFromData(
            chunkData.Blocks,
            m => mesh = m
        );

        if (!world.State.ActiveChunks.TryGetValue(chunkCoord, out var chunkGO))
            yield break;

        world.Renderer.Apply(chunkGO, mesh);
    }

    public void RebuildImmediate(Vector2Int coord)
    {
        RequestRebuild(coord);
    }

    public void Clear()
    {
        rebuildQueue.Clear();
        queuedSet.Clear();
    }
}