using Hexecs.Actors;
using Hexecs.Actors.Pipelines;
using Hexecs.Utils;

namespace Hexecs.Monogame.Features.Pilots.Create;

internal sealed class CreatePilotHandler(ActorContext context)
    : ActorCommandHandler<CreatePilotCommand, Actor<Pilot>>(context)
{
    public override Actor<Pilot> Handle(in CreatePilotCommand command)
    {
        var commandName = command.Name;

        var args = Args.Rent(nameof(Pilot.Name), commandName);
        var pilot = Context.BuildActor<Pilot>(command.Asset, args);

        Log.Info("Pilot '{Name}' created", commandName);

        return pilot;
    }
}