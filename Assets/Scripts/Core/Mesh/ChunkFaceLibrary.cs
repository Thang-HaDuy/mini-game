using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.Core.Mesh
{
    public static class ChunkFaceLibrary
    {
        public static readonly Vector3Int[] Directions =
        {
            Vector3Int.right,
            Vector3Int.left,
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.forward,
            Vector3Int.back
        };

        public static readonly Dictionary<Vector3Int, FaceData> Faces =
            new()
            {
                {
                    Vector3Int.right,
                    new FaceData(
                        new Vector3[]
                        {
                            new(.5f,-.5f,-.5f),
                            new(.5f,-.5f,.5f),
                            new(.5f,.5f,.5f),
                            new(.5f,.5f,-.5f)
                        },
                        new[] {0,2,1,0,3,2},
                        new[] {2,3,1,0}
                    )
                },

                {
                    Vector3Int.left,
                    new FaceData(
                        new Vector3[]
                        {
                            new(-.5f,-.5f,-.5f),
                            new(-.5f,-.5f,.5f),
                            new(-.5f,.5f,.5f),
                            new(-.5f,.5f,-.5f)
                        },
                        new[] {0,1,2,0,2,3},
                        new[] {2,3,1,0}
                    )
                },

                {
                    Vector3Int.up,
                    new FaceData(
                        new Vector3[]
                        {
                            new(-.5f,.5f,-.5f),
                            new(-.5f,.5f,.5f),
                            new(.5f,.5f,.5f),
                            new(.5f,.5f,-.5f)
                        },
                        new[] {0,1,2,0,2,3},
                        new[] {0,1,3,2}
                    )
                },

                {
                    Vector3Int.down,
                    new FaceData(
                        new Vector3[]
                        {
                            new(-.5f,-.5f,-.5f),
                            new(-.5f,-.5f,.5f),
                            new(.5f,-.5f,.5f),
                            new(.5f,-.5f,-.5f)
                        },
                        new[] {0,2,1,0,3,2},
                        new[] {0,1,3,2}
                    )
                },

                {
                    Vector3Int.forward,
                    new FaceData(
                        new Vector3[]
                        {
                            new(-.5f,-.5f,.5f),
                            new(-.5f,.5f,.5f),
                            new(.5f,.5f,.5f),
                            new(.5f,-.5f,.5f)
                        },
                        new[] {0,2,1,0,3,2},
                        new[] {3,1,0,2}
                    )
                },

                {
                    Vector3Int.back,
                    new FaceData(
                        new Vector3[]
                        {
                            new(-.5f,-.5f,-.5f),
                            new(-.5f,.5f,-.5f),
                            new(.5f,.5f,-.5f),
                            new(.5f,-.5f,-.5f)
                        },
                        new[] {0,1,2,0,2,3},
                        new[] {3,1,0,2}
                    )
                }
            };
    }
}