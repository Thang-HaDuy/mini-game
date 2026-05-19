using System;

namespace MiniGame.Core.Generation.Biomes
{
    public interface IStructureRule
    {
        void TryApply(int[,,] blocks, int x, int surfaceY, int z, Random rng);
    }
}
