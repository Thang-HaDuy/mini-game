using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
    private readonly WorldGenerator world;

    private readonly Queue<Vector2Int> rebuildQueue = new();
    private readonly HashSet<Vector2Int> queuedSet = new();

    private readonly int rebuildPerFrame;

    public ChunkManager(WorldGenerator world, int rebuildPerFrame = 1)
    {
        this.world = world;
        this.rebuildPerFrame = rebuildPerFrame;
    }

    public void EnqueueRebuild(Vector2Int coord)
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

            world.StartCoroutine(RebuildChunkRoutine(coord));
            count--;
        }
    }

    private IEnumerator RebuildChunkRoutine(Vector2Int chunkCoord)
    {
        if (!world.State.ActiveChunks.ContainsKey(chunkCoord))
            yield break;

        var chunkData = world.WorldStorage.GetChunk(chunkCoord);
        if (chunkData == null)
            yield break;

        Mesh mesh = null;

        yield return world.MeshCreator.CreateMeshFromData(
            chunkData.Blocks,
            m => mesh = m
        );

        if (!world.State.ActiveChunks.TryGetValue(chunkCoord, out var chunkGO))
            yield break;

        world.ChunkRenderer.Apply(chunkGO, mesh);
    }

    public void RebuildImmediate(Vector2Int coord)
    {
        EnqueueRebuild(coord);
    }

    public void Clear()
    {
        rebuildQueue.Clear();
        queuedSet.Clear();
    }
}