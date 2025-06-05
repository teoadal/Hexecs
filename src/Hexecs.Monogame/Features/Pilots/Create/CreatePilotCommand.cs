using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Pipelines;

namespace Hexecs.Monogame.Features.Pilots.Create;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct CreatePilotCommand(Asset<PilotAsset> asset, string name) : ICommand<Actor<Pilot>>
{
    public readonly Asset<PilotAsset> Asset = asset;
    public readonly string Name = name;
}