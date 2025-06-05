using Hexecs.Assets;

namespace Hexecs.Monogame.Features.Healths;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct HealthAbility(ushort value) : IAssetComponent
{
    public readonly ushort Value = value;
}