using Hexecs.Loggers;

namespace Hexecs.Actors.Loggers;

internal sealed class ActorLogWriter : ILogValueWriter<Actor>, ILogValueWriterFactory
{
    public static readonly ActorLogWriter Instance = new();
    public static ILogValueWriterFactory Factory => Instance;

    private ActorLogWriter()
    {
    }

    public bool TryCreateWriter<T>(out ILogValueWriter<T> writer)
    {
        var type = typeof(T);
        if (type is { IsValueType: true, IsGenericType: true })
        {
            if (type.GetGenericTypeDefinition() == typeof(Actor<>))
            {
                writer = new LikeActorStruct<T>();
                return true;
            }
        }

        writer = null!;
        return false;
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

    private sealed class LikeActorStruct<T> : ILogValueWriter<T>
    {
        public void Write(ref ValueStringBuilder stringBuilder, T arg)
        {
            ref readonly var actor = ref Unsafe.As<T, Actor>(ref arg);
            Instance.Write(ref stringBuilder, actor);
        }
    }
}