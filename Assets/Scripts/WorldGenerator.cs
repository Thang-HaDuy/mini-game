using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    public static readonly Vector3Int ChunkSize = new Vector3Int(16, 256, 16);
    public Vector2 NoiseScale = Vector2.one;
    public Vector2 NoiseOffset = Vector2.zero;
    [Space]
    public int HeightOffset = 60;
    public float HeightIntensity = 5f; 
    private int[,,] TempData;
    void Start()
    {
        TempData = new int[ChunkSize.x, ChunkSize.y, ChunkSize.z];
        
        for(int x = 0; x < ChunkSize.x; x++)
        {
            for(int z = 0; z < ChunkSize.z; z++)
            {
                float PerlinCoordX = NoiseOffset.x + x / (float)ChunkSize.x * NoiseScale.x;
                float PerlinCoordY = NoiseOffset.y + z / (float)ChunkSize.z * NoiseScale.y;
                int HeightGen = Mathf.RoundToInt(Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY) * HeightIntensity + HeightOffset);

                for(int y = HeightGen; y >= 0; y--)
                {
                    int BlockTypeToAssign = 0;

                    //Set first layer to grass
                    if (y == HeightGen) BlockTypeToAssign = 1;

                    //Set next 3 layers to dirt
                    if (y < HeightGen && y > HeightGen - 4) BlockTypeToAssign = 1;

                    //Set everything between the dirt range (inclusive) and 0 (exclusive) to stone
                    if (y <= HeightGen - 4 && y > 0) BlockTypeToAssign = 2;

                    //Set everything at height 0 to bedrock.
                    if (y == 0) BlockTypeToAssign = 3;

                    TempData[x, y, z] = BlockTypeToAssign;
                }
            }
        }

        GameObject TempChunk = new GameObject("Chunk", new System.Type[] { typeof(MeshRenderer), typeof(MeshFilter) });
        TempChunk.GetComponent<MeshFilter>().mesh = new ChunkMeshCreator().CreateMeshFromData(TempData);
    }

    private void OnDrawGizmos()
    {
        if(TempData != null)
        {
            for(int x = 0; x < ChunkSize.x; x++)
            {
                for(int y = 0; y < ChunkSize.y; y++)
                {
                    for(int z = 0; z < ChunkSize.z; z++)
                    {
                        switch (TempData[x, y, z])
                        {
                            case 0:
                                continue;

                            case 1:
                                Gizmos.color = Color.green;
                                break;

                            case 2:
                                Gizmos.color = Color.yellow;
                                break;

                            case 3:
                                Gizmos.color = Color.gray;
                                break;

                            case 4:
                                Gizmos.color = Color.black;
                                break;
                        }

                        Gizmos.DrawWireCube(new Vector3(x, y, z), Vector3.one);
                    }
                }
            }
        }
    }
}