using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.Core.Mesh
{
    public class MeshData
    {
        public readonly List<Vector3> Vertices = new();
        public readonly List<int> Indices = new();
        public readonly List<Vector2> UVs = new();
    }
}