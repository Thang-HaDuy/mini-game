// Core/Mesh/FaceData.cs
using UnityEngine;

namespace MiniGame.Core.Rendering
{
    public class FaceData
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public int[] UVIndexOrder;

        public FaceData(
            Vector3[] vertices,
            int[] indices,
            int[] uvIndexOrder)
        {
            Vertices = vertices;
            Indices = indices;
            UVIndexOrder = uvIndexOrder;
        }
    }
}