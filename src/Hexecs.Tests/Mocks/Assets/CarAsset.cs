using Hexecs.Assets;

namespace Hexecs.Tests.Mocks.Assets;

public readonly struct CarAsset(int price, int speed) : IAssetComponent
{
    public const string Alias1 = "Alias1";
    public const string Alias2 = "Alias2";

    public readonly int Price = price;
    public readonly int Speed = speed;
}