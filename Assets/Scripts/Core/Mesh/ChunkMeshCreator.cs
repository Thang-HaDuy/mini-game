using System;
using System.Collections;
using System.Threading.Tasks;
using MiniGame.Core.Interfaces;
using UnityEngine;

namespace MiniGame.Core.Mesh
{
    public class ChunkMeshCreator : IMeshBuilder
    {
        private TextureLoader textureLoader;

        public ChunkMeshCreator(TextureLoader textureLoaderInstance)
        {
            textureLoader = textureLoaderInstance;
        }

        public IEnumerator CreateMeshFromData(
            int[,,] blocks,
            Action<UnityEngine.Mesh> callback
        )
        {
            MeshData meshData = new();

            Task task = Task.Run(() => GenerateMesh(blocks, meshData));

            yield return new WaitUntil(() => task.IsCompleted);

            UnityEngine.Mesh mesh = BuildUnityMesh(meshData);

            callback(mesh);
        }

        private void GenerateMesh(int[,,] blocks, MeshData meshData)
        {
            int sizeX = blocks.GetLength(0);
            int sizeY = blocks.GetLength(1);
            int sizeZ = blocks.GetLength(2);

            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    for (int z = 0; z < sizeZ; z++)
                        TryBuildBlock(blocks, meshData, x, y, z);
        }

        private void TryBuildBlock(int[,,] blocks, MeshData meshData, int x, int y, int z)
        {
            if (blocks[x, y, z] == 0)
                return;

            foreach (var dir in ChunkFaceLibrary.Directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                int nz = z + dir.z;

                if (ChunkMeshUtility.IsSolid(blocks, nx, ny, nz))
                    continue;

                BuildFace(blocks, meshData, x, y, z, dir);
            }
        }

        private void BuildFace(int[,,] blocks, MeshData meshData, int x, int y, int z, Vector3Int direction)
        {
            int blockId = blocks[x, y, z];

            var texture = textureLoader.Textures[blockId];
            var face = ChunkFaceLibrary.Faces[direction];
            var uvs = texture.GetUVsAtDirectionT(direction);

            ChunkMeshBuilder.AddFace(meshData, face, new Vector3(x, y, z), uvs);
        }

        private UnityEngine.Mesh BuildUnityMesh(MeshData meshData)
        {
            UnityEngine.Mesh mesh = new();

            mesh.SetVertices(meshData.Vertices);
            mesh.SetIndices(meshData.Indices, MeshTopology.Triangles, 0);
            mesh.SetUVs(0, meshData.UVs);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }
    }
}
