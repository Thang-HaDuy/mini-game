using System;
using System.Collections;
using UnityEngine;

namespace MiniGame.Core.Interfaces
{
    public interface IMeshBuilder
    {
        IEnumerator CreateMeshFromData(
            int[,,] blocks,
            Action<Mesh> callback
        );
    }
}