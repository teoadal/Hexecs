using Hexecs.Assets;

namespace Hexecs.Tests.Mocks.Assets;

public readonly struct DecisionAsset(int min, int max) : IAssetComponent
{
    public readonly int Min = min;
    public readonly int Max = max;
}