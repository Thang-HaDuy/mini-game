using UnityEngine;

namespace MiniGame.Core.Interfaces
{
    public interface IChunkSystem
    {
        void RequestChunk(Vector2Int coord);

        void RequestRebuild(Vector2Int coord);

        void ProcessQueue();
    }
}
