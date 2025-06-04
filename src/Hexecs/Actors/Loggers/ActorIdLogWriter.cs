using Hexecs.Loggers;

namespace Hexecs.Actors.Loggers;

internal sealed class ActorIdLogWriter : ILogValueWriter<ActorId>, ILogValueWriterFactory
{
    public static readonly ActorIdLogWriter Instance = new();
    public static ILogValueWriterFactory Factory => Instance;

    private ActorIdLogWriter()
    {
    }

    public bool TryCreateWriter<T>(out ILogValueWriter<T> writer)
    {
        var type = typeof(T);
        if (type is { IsValueType: true, IsGenericType: true })
        {
            if (type.GetGenericTypeDefinition() == typeof(ActorId<>))
            {
                writer = new LikeActorIdStruct<T>();
                return true;
            }
        }

        writer = null!;
        return false;
    }

    public void Write(ref ValueStringBuilder stringBuilder, ActorId actor)
    {
        if (actor.IsEmpty)
        {
            stringBuilder.Append(StringUtils.EmptyValue);
        }
        else
        {
            if (ActorMarshal.TryGetDebugContext(out var context))
            {
                context.GetDescription(actor.Value, ref stringBuilder);
            }
            else
            {
                stringBuilder.Append(actor.Value);
            }
        }
    }
    
    private sealed class LikeActorIdStruct<T> : ILogValueWriter<T>
    {
        public void Write(ref ValueStringBuilder stringBuilder, T arg)
        {
            ref readonly var actor = ref Unsafe.As<T, ActorId>(ref arg);
            Instance.Write(ref stringBuilder, actor);
        }
    }
}