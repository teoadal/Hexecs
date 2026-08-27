using Hexecs.Loggers;

namespace Hexecs.Actors.Loggers;

internal sealed class ActorLogWriter : ILogValueWriter<Actor>
{
    public static readonly ActorLogWriter Instance = new ActorLogWriter();

    private ActorLogWriter()
    {
    }

    public void Write(ref ValueStringBuilder stringBuilder, Actor actor)
    {
        if (actor.IsEmpty)
        {
            stringBuilder.Append(StringUtils.EmptyValue);
        }
        else
        {
            actor.Context.GetDescription(actor.Id, ref stringBuilder);
        }
    }
}
