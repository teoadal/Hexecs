using Hexecs.Assets;

namespace Hexecs.Monogame.Features.Pilots;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct PilotAsset(string name) : IAssetComponent
{
    public const string Ace = "Пилот ас";
    public const string Common = "Обычный";
    public const string Newbie = "Новичёк";
    
    public readonly string Name = name;
}