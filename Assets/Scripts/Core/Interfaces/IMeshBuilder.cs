using System;
using System.Collections;

namespace MiniGame.Core.Interfaces
{
    public interface IMeshBuilder
    {
        IEnumerator CreateMeshFromData(
            int[,,] blocks,
            Action<UnityEngine.Mesh> callback
        );
    }
}