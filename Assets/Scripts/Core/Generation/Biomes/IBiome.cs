namespace MiniGame.Core.Generation.Biomes
{
    public interface IBiome
    {
        bool Matches(float temperature, float humidity);
        int GetBlockAt(int y, int surfaceHeight);
        float HeightMultiplier { get; }
        float BaseElevation { get; }
        IStructureRule[] Structures { get; }
    }
}
