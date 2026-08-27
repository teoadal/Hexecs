using Hexecs.Actors;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Spawn.Components;
using Hexecs.Utils;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Spawn.Systems;

internal sealed class DestroySystem : UpdateSystem
{
    private readonly ActorFilter<Lifetime> _filter;

    public DestroySystem(ActorContext context) : base(context)
    {
        _filter = context.Filter<Lifetime>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public override void Update(in WorldTime time)
    {
        ComponentsAccess<Lifetime> components = _filter.GetComponents1();

        foreach (uint actorId in _filter.Keys)
        {
            if (components[actorId].Value <= 0f)
            {
                Context.DestroyActor(ActorId.Unsafe(actorId));
            }
        }
    }
}
