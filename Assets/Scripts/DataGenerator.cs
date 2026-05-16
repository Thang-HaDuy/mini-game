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

    private WorldGenerator GeneratorInstance;
    private Queue<GenData> DataToGenerate;
    public bool Terminate;

    private StructureGenerator structureGen;
    public DataGenerator(WorldGenerator worldGen, StructureGenerator structureGen = null)
    {
        GeneratorInstance = worldGen;
        DataToGenerate = new Queue<GenData>();
        this.structureGen = structureGen;

        worldGen.StartCoroutine(DataGenLoop());
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
                yield return GeneratorInstance.StartCoroutine(GenerateData(gen.GenerationPoint, gen.OnComplete));
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
            WorldGenerator.ChunkSize;

        Vector2 noiseOffset =
            GeneratorInstance.NoiseOffset;

        Vector2 noiseScale =
            GeneratorInstance.NoiseScale;

        float heightIntensity =
            GeneratorInstance.HeightIntensity;

        float heightOffset =
            GeneratorInstance.HeightOffset;

        ChunkData chunkData = GeneratorInstance.Storage.GetChunk(offset);

        int[,,] tempData;

        if (chunkData != null)
        {
            tempData = chunkData.Blocks;
        }
        else
        {
            chunkData = new ChunkData(offset);
            tempData = chunkData.Blocks;
        }
 
        Task t = Task.Factory.StartNew(() =>
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int z = 0; z < chunkSize.z; z++)
                {
                    float perlinCoordX =
                        noiseOffset.x +
                        (
                            x +
                            (offset.x * 16f)
                        )
                        / chunkSize.x
                        * noiseScale.x;

                    float perlinCoordY =
                        noiseOffset.y +
                        (
                            z +
                            (offset.y * 16f)
                        )
                        / chunkSize.z
                        * noiseScale.y;

                    int heightGen =
                        Mathf.RoundToInt(
                            Mathf.PerlinNoise(
                                perlinCoordX,
                                perlinCoordY
                            )
                            * heightIntensity
                            + heightOffset
                        );

                    float biomeCoordX =
                        noiseOffset.x +
                        (
                            x +
                            (offset.x * 16f)
                        )
                        / chunkSize.x
                        * 0.75f;

                    float biomeCoordY =
                        noiseOffset.y +
                        (
                            z +
                            (offset.y * 16f)
                        )
                        / chunkSize.z
                        * 0.75f;

                    float biomeNoise =
                        Mathf.PerlinNoise(
                            biomeCoordX,
                            biomeCoordY
                        );

                    BiomeData data;

                    if (biomeNoise < 0.5f)
                    {
                        data = new BiomeData
                        {
                            topBlock = 4,
                            topMiddleBlock = 2,
                            bottomMiddleBlock = 3,
                            bottomBlock = 4
                        };
                    }
                    else
                    {
                        data = new BiomeData
                        {
                            topBlock = 3,
                            topMiddleBlock = 2,
                            bottomMiddleBlock = 3,
                            bottomBlock = 1
                        };
                    }

                    for (
                        int y = heightGen;
                        y >= 0;
                        y--
                    )
                    {
                        int blockTypeToAssign = 0;

                        if (y == heightGen)
                            blockTypeToAssign =
                                data.topBlock;

                        if (
                            y < heightGen &&
                            y > heightGen - 4
                        )
                        {
                            blockTypeToAssign =
                                data.topMiddleBlock;
                        }

                        if (
                            y <= heightGen - 4 &&
                            y > 0
                        )
                        {
                            blockTypeToAssign =
                                data.bottomMiddleBlock;
                        }

                        if (y == 0)
                        {
                            blockTypeToAssign =
                                data.bottomBlock;
                        }

                        if (
                            tempData[x, y, z] == 0
                        )
                        {
                            tempData[x, y, z] =
                                blockTypeToAssign;
                        }
                    }

                    if (
                        structureGen != null &&
                        data.topBlock == 4
                    )
                    {
                        structureGen.GenerateStructure(
                            offset,
                            ref tempData,
                            x,
                            z
                        );
                    }
                }
            }
        });

        yield return new WaitUntil(() =>
        {
            return t.IsCompleted || t.IsCanceled;
        });

        if (t.Exception != null)
        {
            Debug.LogError(t.Exception);
        }

        GeneratorInstance.Storage.AddChunk(
            chunkData
        );

        callback(chunkData);
    }
}
