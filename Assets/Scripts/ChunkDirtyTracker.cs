using System.Collections.Generic;
using UnityEngine;

public static class ChunkDirtyTracker
{
    private static HashSet<Vector2Int> dirtyChunks = new();

    public static void MarkDirty(Vector2Int coord)
    {
        dirtyChunks.Add(coord);
    }

    public static bool TryConsume(out Vector2Int coord)
    {
        foreach (var c in dirtyChunks)
        {
            coord = c;
            dirtyChunks.Remove(c);
            return true;
        }

        coord = default;
        return false;
    }

    public static bool HasDirty => dirtyChunks.Count > 0;
}