using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private int RenderDistance;

    private WorldGenerator GeneratorInstance;

    private List<Vector2Int> CoordsToRemove;

    void Start()
    {
        GeneratorInstance = GetComponent<WorldGenerator>();
        CoordsToRemove = new List<Vector2Int>();
    }

    void Update()
    {
        int plrChunkX = (int)Player.position.x / WorldGenerator.ChunkSize.x;
        int plrChunkY = (int)Player.position.z / WorldGenerator.ChunkSize.z;

        CoordsToRemove.Clear();

        var activeChunks = GeneratorInstance.State.ActiveChunks;

        // CHỈ COLLECT CHUNK OUTSIDE RANGE
        foreach (var kv in activeChunks)
        {
            Vector2Int coord = kv.Key;

            if (Mathf.Abs(coord.x - plrChunkX) > RenderDistance ||
                Mathf.Abs(coord.y - plrChunkY) > RenderDistance)
            {
                CoordsToRemove.Add(coord);
            }
        }

        // LOAD / REBUILD CHUNKS TRONG RANGE
        for (int x = plrChunkX - RenderDistance; x <= plrChunkX + RenderDistance; x++)
        {
            for (int y = plrChunkY - RenderDistance; y <= plrChunkY + RenderDistance; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);

                if (!activeChunks.ContainsKey(chunkCoord))
                {
                    GeneratorInstance.ChunkManager.RequestChunk(chunkCoord);
                }
            }
        }

        // REMOVE OUT OF RANGE CHUNKS
        foreach (Vector2Int coord in CoordsToRemove)
        {
            if (activeChunks.TryGetValue(coord, out GameObject chunkToDelete))
            {
                activeChunks.Remove(coord);
                Destroy(chunkToDelete);
            }
        }
    }
}