using System.Collections;
using MiniGame.Core.Context;
using MiniGame.Core.Data;
using UnityEngine;

namespace MiniGame.Core.Pipeline
{
    public class ChunkPipeline
    {
        private readonly WorldContext world;

        public ChunkPipeline(WorldContext world)
        {
            this.world = world;
        }

        public IEnumerator BuildChunk(Vector2Int coord)
        {
            ChunkData data = world.Storage.GetChunk(coord);

            if (data == null)
            {
                yield return world.DataGenerator.GenerateData(coord, d => data = d);

                if (data == null)
                    yield break;
            }

            if (!world.State.ActiveChunks.TryGetValue(coord, out var go))
            {
                go = CreateChunkObject(coord);
                world.State.ActiveChunks[coord] = go;
            }

            yield return ApplyMesh(coord, data);
        }

        public IEnumerator RebuildChunk(Vector2Int coord)
        {
            if (!world.State.ActiveChunks.ContainsKey(coord))
                yield break;

            var data = world.Storage.GetChunk(coord);
            if (data == null)
                yield break;

            yield return ApplyMesh(coord, data);
        }

        private IEnumerator ApplyMesh(Vector2Int coord, ChunkData data)
        {
            UnityEngine.Mesh mesh = null;

            yield return world.MeshCreator.CreateMeshFromData(data.Blocks, m => mesh = m);

            if (world.State.ActiveChunks.TryGetValue(coord, out var go))
                world.Renderer.Apply(go, mesh);
        }

        private GameObject CreateChunkObject(Vector2Int coord)
        {
            var go = new GameObject($"Chunk_{coord.x}_{coord.y}");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.AddComponent<MeshCollider>();

            Vector3Int size = world.Config.ChunkSize;
            go.transform.position = new Vector3(coord.x * size.x, 0, coord.y * size.z);

            return go;
        }
    }
}
