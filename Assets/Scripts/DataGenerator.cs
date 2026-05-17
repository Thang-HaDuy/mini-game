using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class DataGenerator
{
    public struct BiomeData
    {
        public int topBlock;
        public int topMiddleBlock;
        public int bottomMiddleBlock;
        public int bottomBlock;
    }

    public class GenData
    {
        public System.Action<ChunkData> OnComplete;
        public Vector2Int GenerationPoint;
    }
    private readonly WorldContext world;
    private Queue<GenData> DataToGenerate;
    public bool Terminate;

    private StructureGenerator structureGen;
    public DataGenerator(WorldContext world, StructureGenerator structureGen = null)
    {
        this.world = world;
        DataToGenerate = new Queue<GenData>();
        this.structureGen = structureGen;

        world.Runtime.RunCoroutine(DataGenLoop());
    }

    public void QueueDataToGenerate(GenData data)
    {
        DataToGenerate.Enqueue(data);
    }

    public IEnumerator DataGenLoop()
    {
        while (Terminate == false)
        {
            if (DataToGenerate.Count > 0)
            {
                GenData gen = DataToGenerate.Dequeue();
                yield return world.Runtime.RunCoroutine(GenerateData(gen.GenerationPoint, gen.OnComplete));
            }

            yield return null;
        }
    }
    public IEnumerator GenerateData(
        Vector2Int offset,
        System.Action<ChunkData> callback
    )
    {
        Vector3Int chunkSize =
            world.Config.ChunkSize;

        Vector2 noiseOffset =
            world.Config.NoiseOffset;

        Vector2 noiseScale =
            world.Config.NoiseScale;

        float heightIntensity =
            world.Config.HeightIntensity;

        float heightOffset =
            world.Config.HeightOffset;

        ChunkData chunkData = world.Storage.GetChunk(offset);

        int[,,] tempData;

        if (chunkData != null)
        {
            tempData = chunkData.Blocks;
        }
        else
        {
            chunkData = new ChunkData(offset, chunkSize);
            tempData = chunkData.Blocks;
        }

        Task t = Task.Factory.StartNew(() =>
{
    for (int x = 0; x < chunkSize.x; x++)
    {
        for (int z = 0; z < chunkSize.z; z++)
        {
            float perlinCoordX = noiseOffset.x + (x + (offset.x * 16f)) / chunkSize.x * noiseScale.x;
            float perlinCoordY = noiseOffset.y + (z + (offset.y * 16f)) / chunkSize.z * noiseScale.y;

            int heightGen = Mathf.RoundToInt(
                Mathf.PerlinNoise(perlinCoordX, perlinCoordY) * heightIntensity + heightOffset
            );

            float biomeNoise = Mathf.PerlinNoise(perlinCoordX * 0.75f, perlinCoordY * 0.75f);

            BiomeData data = biomeNoise < 0.5f
                ? new BiomeData { topBlock = 4, topMiddleBlock = 2, bottomMiddleBlock = 3, bottomBlock = 4 }
                : new BiomeData { topBlock = 3, topMiddleBlock = 2, bottomMiddleBlock = 3, bottomBlock = 1 };

            for (int y = heightGen; y >= 0; y--)
            {
                int blockType = 0;

                if (y == heightGen) blockType = data.topBlock;
                else if (y > heightGen - 4) blockType = data.topMiddleBlock;
                else if (y > 0) blockType = data.bottomMiddleBlock;
                else blockType = data.bottomBlock;

                if (tempData[x, y, z] == 0)
                    tempData[x, y, z] = blockType;
            }
        }
    }
});

        yield return new WaitUntil(() => t.IsCompleted);

        if (t.Exception != null)
            Debug.LogError(t.Exception);

        // MAIN THREAD ONLY
        world.Storage.AddChunk(chunkData);

        if (structureGen != null)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {
                    structureGen.GenerateStructure(offset, ref chunkData.Blocks, x, z);
                }
            }
        }

        callback(chunkData);
    }
}
