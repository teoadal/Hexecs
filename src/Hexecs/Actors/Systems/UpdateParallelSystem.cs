using Hexecs.Utils;
using Hexecs.Worlds;

namespace Hexecs.Actors.Systems;

internal sealed class UpdateParallelSystem : IUpdateSystem, IHaveOrder
{
    public readonly ActorContext Context;

    public bool Enabled { get; set; } = true;

    public int Order { get; }

    private readonly IUpdateSystem[] _systems;
    private readonly Action[] _systemUpdates;
    private readonly Task[] _tasks;

    private WorldTime _currentTime;

    public UpdateParallelSystem(ActorContext context, int order, IUpdateSystem[] systems)
    {
        Context = context;
        Order = order;

        var systemActions = new Action[systems.Length];
        for (var i = 0; i < systems.Length; i++)
        {
            var system = systems[i];
            systemActions[i] = () => { system.Update(_currentTime); };
        }

        _systems = systems;
        _systemUpdates = systemActions;
        _tasks = new Task[systems.Length];
    }

    public bool TryGetSystem<T>([NotNullWhen(true)] out T? system) where T : class, IUpdateSystem
    {
        foreach (var exists in _systems)
        {
            if (exists is not T expected) continue;

            system = expected;
            return true;
        }

        system = null;
        return false;
    }

    public void Update(in WorldTime time)
    {
        if (!Enabled) return;

        _currentTime = time;
        for (var i = 0; i < _systemUpdates.Length; i++)
        {
            _tasks[i] = Task.Run(_systemUpdates[i]);
        }

        Task.WaitAll(_tasks);
        ArrayUtils.Clear(_tasks);
    }

    ActorContext IUpdateSystem.Context => Context;
}