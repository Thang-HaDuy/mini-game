using UnityEngine;

namespace MiniGame.Core.Mesh
{
    public static class ChunkMeshBuilder
    {
        public static void AddFace(
            MeshData meshData,
            FaceData face,
            Vector3 blockPos,
            Vector2[] uvs)
        {
            foreach (var vert in face.Vertices)
            {
                meshData.Vertices.Add(blockPos + vert);
            }

            foreach (var tri in face.Indices)
            {
                meshData.Indices.Add(
                    meshData.Vertices.Count - 4 + tri
                );
            }

            foreach (var uvIndex in face.UVIndexOrder)
            {
                meshData.UVs.Add(uvs[uvIndex]);
            }
        }
    }
}