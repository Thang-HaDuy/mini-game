using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    public readonly Dictionary<Vector2Int, GameObject> ActiveChunks = new();
}