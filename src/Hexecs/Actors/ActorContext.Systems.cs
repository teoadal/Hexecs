using Hexecs.Actors.Systems;
using Hexecs.Worlds;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private IDrawSystem[] _drawSystems;
    private IUpdateSystem[] _updateSystems;

    /// <summary>
    /// Выполняет отрисовку всех актёров.
    /// </summary>
    /// <param name="time">Время мира.</param>
    public void Draw(in WorldTime time)
    {
        foreach (var system in _drawSystems)
        {
            system.Draw(in time);
        }
    }

    /// <summary>
    /// Возвращает систему отрисовки заданного типа.
    /// </summary>
    /// <typeparam name="T">Тип системы.</typeparam>
    /// <returns>Система отрисовки</returns>
    /// <exception cref="Exception">Если система не найдена</exception>
    public T GetDrawSystem<T>() where T : class, IDrawSystem
    {
        foreach (var exists in _drawSystems)
        {
            if (exists is T expected) return expected;
        }

        ActorError.DrawSystemNotFound<T>();
        return null;
    }
    
    public T GetUpdateSystem<T>() where T : class, IUpdateSystem
    {
        foreach (var exists in _updateSystems)
        {
            switch (exists)
            {
                case T expected:
                    return expected;
                case ParallelSystem parallelSystem when parallelSystem.TryGetSystem<T>(out var parallel):
                    return parallel;
            }
        }

        ActorError.UpdateSystemNotFound<T>();
        return null;
    }

    public bool TryGetDrawSystem<T>([NotNullWhen(true)] out T? system) where T : class, IDrawSystem
    {
        foreach (var exists in _drawSystems)
        {
            if (exists is not T expected) continue;
            
            system = expected;
            return true;
        }

        system = null;
        return false;
    }
    
    public bool TryGetUpdateSystem<T>([NotNullWhen(true)] out T? system) where T : class, IUpdateSystem
    {
        foreach (var exists in _updateSystems)
        {
            switch (exists)
            {
                case T expected:
                    system = expected;
                    return true;
                case ParallelSystem parallelSystem when parallelSystem.TryGetSystem<T>(out var parallel):
                    system = parallel;
                    return true;
            }
        }

        system = null;
        return false;
    }

    public void Update(in WorldTime time)
    {
        foreach (var system in _updateSystems)
        {
            system.Update(in time);
        }
    }

    internal void LoadSystems(IEnumerable<IDrawSystem> drawSystems, IEnumerable<IUpdateSystem> updateSystems)
    {
        _drawSystems = drawSystems.ToArray();
        _updateSystems = updateSystems.ToArray();
    }
}