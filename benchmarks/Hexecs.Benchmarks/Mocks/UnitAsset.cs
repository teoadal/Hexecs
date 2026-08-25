using Hexecs.Assets;

namespace Hexecs.Benchmarks.Mocks;

public readonly struct UnitAsset(int attack, int defence) : IAssetComponent
{
    public const string Alias1 = "Alias1";
    public const string Alias2 = "Alias2";

    public readonly int Attack = attack;
    public readonly int Defence = defence;
}