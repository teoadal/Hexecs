using Hexecs.Assets;

namespace Hexecs.Monogame.Features.Planes;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct PlaneAsset(string name) : IAssetComponent
{
    public const string Big = "Большой";
    public const string Common = "Обычный";
    public const string Small = "Маленький";
    
    public readonly string Name = name;
}