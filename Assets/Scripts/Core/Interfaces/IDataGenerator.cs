using System;
using System.Collections;
using MiniGame.Core.Data;
using UnityEngine;

namespace MiniGame.Core.Interfaces
{
    public interface IDataGenerator
    {
        IEnumerator GenerateData(
            Vector2Int chunkCoord,
            Action<ChunkData> callback
        );
    }
}