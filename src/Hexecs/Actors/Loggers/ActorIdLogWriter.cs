using Hexecs.Loggers;

namespace Hexecs.Actors.Loggers;

internal sealed class ActorIdLogWriter : ILogValueWriter<ActorId>
{
    public static readonly ActorIdLogWriter Instance = new ActorIdLogWriter();

    private ActorIdLogWriter()
    {
    }

    public void Write(ref ValueStringBuilder stringBuilder, ActorId actor)
    {
        if (actor.IsEmpty)
        {
            stringBuilder.Append(StringUtils.EmptyValue);
        }
        else
        {
            if (ActorMarshal.TryGetDebugContext(out ActorContext? context))
            {
                context.GetDescription(actor, ref stringBuilder);
            }
            else
            {
                stringBuilder.Append(actor.Value);
            }
        }
    }
}
