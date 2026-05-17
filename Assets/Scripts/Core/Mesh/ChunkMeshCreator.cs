using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniGame.Core.Context;
using MiniGame.Core.Interfaces;
using UnityEngine;

namespace MiniGame.Core.Mesh
{
    public class ChunkMeshCreator : IMeshBuilder
    {
        public class CreateMesh
        {
            public int[,,] DataToDraw;
            public Action<UnityEngine.Mesh> OnComplete;
        }

        #region FaceData

        static readonly Vector3Int[] CheckDirections = new Vector3Int[]
        {
        Vector3Int.right,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.forward,
        Vector3Int.back
        };

        static readonly Vector3[] RightFace = new Vector3[]
        {
        new Vector3(.5f, -.5f, -.5f),
        new Vector3(.5f, -.5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f)
        };

        static readonly int[] RightTris = new int[]
        {
        0,2,1,0,3,2
        };

        static readonly Vector3[] LeftFace = new Vector3[]
        {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(-.5f, .5f, -.5f)
        };

        static readonly int[] LeftTris = new int[]
        {
        0,1,2,0,2,3
        };

        static readonly Vector3[] UpFace = new Vector3[]
        {
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f)
        };

        static readonly int[] UpTris = new int[]
        {
        0,1,2,0,2,3
        };

        static readonly Vector3[] DownFace = new Vector3[]
        {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(.5f, -.5f, .5f),
        new Vector3(.5f, -.5f, -.5f)
        };

        static readonly int[] DownTris = new int[]
        {
        0,2,1,0,3,2
        };

        static readonly Vector3[] ForwardFace = new Vector3[]
        {
        new Vector3(-.5f, -.5f, .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, .5f),
        new Vector3(.5f, -.5f, .5f)
        };

        static readonly int[] ForwardTris = new int[]
        {
        0,2,1,0,3,2
        };

        static readonly Vector3[] BackFace = new Vector3[]
        {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(.5f, .5f, -.5f),
        new Vector3(.5f, -.5f, -.5f)
        };

        static readonly int[] BackTris = new int[]
        {
        0,1,2,0,2,3
        };

        #endregion

        #region FaceUVData

        static readonly int[] XUVOrder = new int[]
        {
        2, 3, 1, 0
        };

        static readonly int[] YUVOrder = new int[]
        {
        0, 1, 3, 2
        };


        static readonly int[] ZUVOrder = new int[]
        {
        3, 1, 0, 2
        };

        #endregion

        private Dictionary<Vector3Int, FaceData> CubeFaces = new Dictionary<Vector3Int, FaceData>();
        private TextureLoader textureLoader;
        public bool Terminate;

        public ChunkMeshCreator(TextureLoader textureLoaderInstance)
        {
            CubeFaces = new Dictionary<Vector3Int, FaceData>();
            textureLoader = textureLoaderInstance;

            for (int i = 0; i < CheckDirections.Length; i++)
            {
                if (CheckDirections[i] == Vector3Int.up)
                {
                    CubeFaces.Add(CheckDirections[i], new FaceData(UpFace, UpTris, YUVOrder));
                }
                else if (CheckDirections[i] == Vector3Int.down)
                {
                    CubeFaces.Add(CheckDirections[i], new FaceData(DownFace, DownTris, YUVOrder));
                }
                else if (CheckDirections[i] == Vector3Int.forward)
                {
                    CubeFaces.Add(CheckDirections[i], new FaceData(ForwardFace, ForwardTris, ZUVOrder));
                }
                else if (CheckDirections[i] == Vector3Int.back)
                {
                    CubeFaces.Add(CheckDirections[i], new FaceData(BackFace, BackTris, ZUVOrder));
                }
                else if (CheckDirections[i] == Vector3Int.left)
                {
                    CubeFaces.Add(CheckDirections[i], new FaceData(LeftFace, LeftTris, XUVOrder));
                }
                else if (CheckDirections[i] == Vector3Int.right)
                {
                    CubeFaces.Add(CheckDirections[i], new FaceData(RightFace, RightTris, XUVOrder));
                }
            }

        }

        public IEnumerator CreateMeshFromData(
            int[,,] blocks,
            Action<UnityEngine.Mesh> callback
        )
        {
            MeshData meshData = new();

            Task task = Task.Factory.StartNew(() =>
            {
                GenerateMesh(blocks, meshData);
            });

            yield return new WaitUntil(() => task.IsCompleted);

            UnityEngine.Mesh mesh = BuildUnityMesh(meshData);

            callback(mesh);
        }

        private void GenerateMesh(
            int[,,] blocks,
            MeshData meshData
        )
        {
            for (int x = 0; x < WorldGenerator.ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldGenerator.ChunkSize.y; y++)
                {
                    for (int z = 0; z < WorldGenerator.ChunkSize.z; z++)
                    {
                        TryBuildBlock(blocks, meshData, x, y, z);
                    }
                }
            }
        }

        private void TryBuildBlock(
            int[,,] blocks,
            MeshData meshData,
            int x,
            int y,
            int z
        )
        {
            if (blocks[x, y, z] == 0)
                return;

            foreach (var dir in ChunkFaceLibrary.Directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                int nz = z + dir.z;

                bool neighborSolid =
                    ChunkMeshUtility.IsSolid(
                        blocks,
                        nx,
                        ny,
                        nz);

                if (neighborSolid)
                    continue;

                BuildFace(blocks, meshData, x, y, z, dir);
            }
        }

        private void BuildFace(
            int[,,] blocks,
            MeshData meshData,
            int x,
            int y,
            int z,
            Vector3Int direction
        )
        {
            int blockId = blocks[x, y, z];

            var texture =
                textureLoader.Textures[blockId];

            var face =
                ChunkFaceLibrary.Faces[direction];

            Vector2[] uvs =
                texture.GetUVsAtDirectionT(direction);

            ChunkMeshBuilder.AddFace(
                meshData,
                face,
                new Vector3(x, y, z),
                uvs);
        }

        private UnityEngine.Mesh BuildUnityMesh(MeshData meshData)
        {
            UnityEngine.Mesh mesh = new();

            mesh.SetVertices(meshData.Vertices);
            mesh.SetIndices(
                meshData.Indices,
                MeshTopology.Triangles,
                0);

            mesh.SetUVs(0, meshData.UVs);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

    }

}