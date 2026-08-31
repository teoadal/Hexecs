using Hexecs.Benchmarks.Spawn.Components;

using Hexecsm;
using Hexecsm.Accessors;
using Hexecsm.Systems;
using Hexecsm.Worlds;

namespace Hexecs.Benchmarks.Spawn.Systems;

internal sealed class DestroySystem(World world) : ParallelUpdateSystem<Lifetime>(world)
{
    private readonly World _world = world;

    [SkipLocalsInit]
    protected override void Update(
        KeyAccessor batchKeys,
        in ValueAccessor<Lifetime> components1,
        in WorldTime worldTime)
    {
        foreach (ActorId actorId in batchKeys.AsReadOnlySpan())
        {
            ref readonly Lifetime component = ref components1.GetValue(actorId);

            if (component.Value <= 0f)
            {
                _world.DestroyActor(actorId);
            }
        }
    }
}
