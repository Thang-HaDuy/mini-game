using MiniGame.Core.Config;
using MiniGame.Core.Data;
using MiniGame.Core.Generation;
using MiniGame.Core.Interfaces;
using MiniGame.Core.Mesh;
using MiniGame.Core.Pipeline;
using MiniGame.Core.States;
using MiniGame.Core.Streaming;
using UnityEngine;

namespace MiniGame.Core.Context
{
    public class WorldContext
    {
        public WorldStorage Storage;
        public WorldState State;
        public ChunkPipeline ChunkPipeline;
        public ChunkStreamingSystem StreamingSystem;
        public IChunkSystem ChunkManager;
        public IMeshBuilder MeshCreator;
        public IDataGenerator DataGenerator;
        public ChunkRenderer Renderer;
        public StructureGenerator StructureGenerator;
        public WorldConfig Config;

        public RuntimeHost Runtime;
    }
}