using UnityEngine;

public class WorldContext
{
    public WorldStorage Storage;
    public WorldState State;
    public ChunkManager ChunkManager;
    public ChunkPipeline ChunkPipeline;
    public ChunkStreamingSystem StreamingSystem;
    public ChunkMeshCreator MeshCreator;
    public DataGenerator DataGenerator;
    public ChunkRenderer Renderer;
    public StructureGenerator StructureGenerator;
    public WorldConfig Config;

    public RuntimeHost Runtime;
}