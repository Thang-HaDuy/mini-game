using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    public readonly WorldStorage Storage
        = new();

    public readonly Dictionary<Vector2Int, GameObject> ActiveChunks
        = new();

    public readonly Dictionary<Vector2Int, int[,,]> AdditiveWorldData
        = new();
}