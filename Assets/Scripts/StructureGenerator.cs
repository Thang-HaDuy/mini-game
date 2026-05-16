using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    [System.Serializable]
    public class StructureBlockInfo {
        public Vector3Int offsetFromOPoint;
        public int typeToAssign;
    };

    private WorldGenerator world;

    [SerializeField] private StructureBlockInfo[] StructureInfo;

    [Range(0f, 1f)]
    [SerializeField] private float genThreshold;
    private System.Random randomGen;

    private void Awake() {
        randomGen = new System.Random(1337);
    }

    public void Init(WorldGenerator worldGenerator)
    {
        world = worldGenerator;
    }

    private void applyStructure(ref int[,,] dataToModify, Vector2Int originCoords, int x, int y, int z) {
        for (int i = 0; i < StructureInfo.Length; i++) {
            StructureBlockInfo info = StructureInfo[i];
            Vector3Int p = new Vector3Int {
                x = x + info.offsetFromOPoint.x,
                y = y + info.offsetFromOPoint.y,
                z = z + info.offsetFromOPoint.z
            };

            try {
                dataToModify[p.x, p.y, p.z] = info.typeToAssign;
            }
            catch (System.IndexOutOfRangeException) {
                //Debug.LogWarning($"Structure block at {p} is out of range and will not be generated.");
            }
        }
    }

    private int getTopBlockFromDataXZ(int[,,] data, int x, int z) {
        for (int y = WorldGenerator.ChunkSize.y - 1; y >= 0; y--) {
            if (data[x, y, z] != 0) {
                return y;
            }
        }

        return -1;
    }

    public void GenerateStructure(Vector2Int chunkCoords, ref int[,,] dataToModify, int x, int z) {
        float randomValue = (float)randomGen.NextDouble();
        if (randomValue >= genThreshold) {
            applyStructure(ref dataToModify, chunkCoords, x, getTopBlockFromDataXZ(dataToModify, x, z), z);
        }
    }
}