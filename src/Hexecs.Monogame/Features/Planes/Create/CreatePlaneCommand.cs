using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Pipelines;

namespace Hexecs.Monogame.Features.Planes.Create;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct CreatePlaneCommand(Asset<PlaneAsset> asset, string name) : ICommand<Actor<Plane>>
{
    public readonly Asset<PlaneAsset> Asset = asset;
    public readonly string Name = name;
}