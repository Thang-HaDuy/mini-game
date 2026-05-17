using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.Core.States
{
    public class WorldState
    {
        public readonly Dictionary<Vector2Int, GameObject> ActiveChunks = new();
    }
}