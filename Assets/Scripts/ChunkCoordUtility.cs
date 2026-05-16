using UnityEngine;

public static class ChunkCoordUtility
{
    public static Vector2Int WorldToChunk(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / WorldGenerator.ChunkSize.x),
            Mathf.FloorToInt(worldPosition.z / WorldGenerator.ChunkSize.z)
        );
    }

    public static Vector3Int WorldToLocal(
        Vector3Int worldPosition,
        Vector2Int chunkCoords
    )
    {
        return new Vector3Int(
            worldPosition.x - (chunkCoords.x * WorldGenerator.ChunkSize.x),
            worldPosition.y,
            worldPosition.z - (chunkCoords.y * WorldGenerator.ChunkSize.z)
        );
    }

    public static Vector3Int LocalToWorld(
        Vector3Int localPosition,
        Vector2Int chunkCoords
    )
    {
        return new Vector3Int(
            localPosition.x + (chunkCoords.x * WorldGenerator.ChunkSize.x),
            localPosition.y,
            localPosition.z + (chunkCoords.y * WorldGenerator.ChunkSize.z)
        );
    }
}