using UnityEngine;

public static class ChunkMath
{
    public static Vector2Int WorldToChunk(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / WorldGenerator.ChunkSize.x),
            Mathf.FloorToInt(worldPos.z / WorldGenerator.ChunkSize.z)
        );
    }

    public static Vector3Int WorldToLocal(Vector3Int worldPos, Vector2Int chunk)
    {
        return new Vector3Int(
            worldPos.x - chunk.x * WorldGenerator.ChunkSize.x,
            worldPos.y,
            worldPos.z - chunk.y * WorldGenerator.ChunkSize.z
        );
    }

    public static Vector3 ChunkToWorld(Vector2Int chunk)
    {
        return new Vector3(
            chunk.x * WorldGenerator.ChunkSize.x,
            0,
            chunk.y * WorldGenerator.ChunkSize.z
        );
    }
}