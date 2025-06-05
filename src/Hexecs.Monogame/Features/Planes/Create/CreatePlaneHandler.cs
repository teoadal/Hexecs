using Hexecs.Actors;
using Hexecs.Actors.Pipelines;
using Hexecs.Monogame.Features.Pilots;
using Hexecs.Utils;

namespace Hexecs.Monogame.Features.Planes.Create;

internal sealed class CreatePlaneHandler(ActorContext context) : ActorCommandHandler<CreatePlaneCommand, Actor<Plane>>(context)
{
    public override Actor<Plane> Handle(in CreatePlaneCommand command)
    {
        var commandName = command.Name;

        var args = Args.Rent(nameof(Pilot.Name), commandName);
        var plane = Context.BuildActor<Plane>(command.Asset, args);

        Log.Info("Plane '{Name}' created", commandName);

        return plane;
    }
}