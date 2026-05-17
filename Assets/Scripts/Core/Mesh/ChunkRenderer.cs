using UnityEngine;

namespace MiniGame.Core.Mesh
{
    public class ChunkRenderer
    {
        private Material chunkMaterial;

        public ChunkRenderer(Material material)
        {
            chunkMaterial = material;
        }

        public void Apply(GameObject chunkObject, UnityEngine.Mesh mesh)
        {
            if (chunkObject == null || mesh == null) return;

            var filter = chunkObject.GetComponent<MeshFilter>();
            var collider = chunkObject.GetComponent<MeshCollider>();
            var renderer = chunkObject.GetComponent<MeshRenderer>();

            filter.mesh = mesh;
            collider.sharedMesh = mesh;
            renderer.material = chunkMaterial;
        }
    }
}