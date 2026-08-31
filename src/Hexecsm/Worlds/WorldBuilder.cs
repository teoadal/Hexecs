using Hexecsm.Systems;

namespace Hexecsm.Worlds;

public sealed class WorldBuilder
{
    private int? _degreeOfParallelism;
    private int? _initialCapacity;

    private readonly List<Entry<IDrawSystem>> _drawSystems = [];
    private readonly List<Entry<IUpdateSystem>> _updateSystems = [];

    public WorldBuilder AddDrawSystem(Func<World, IDrawSystem> drawSystem)
    {
        _drawSystems.Add(new Entry<IDrawSystem>(drawSystem));

        return this;
    }

    public WorldBuilder AddUpdateSystem(Func<World, IUpdateSystem> updateSystem)
    {
        _updateSystems.Add(new Entry<IUpdateSystem>(updateSystem));

        return this;
    }

    #region Settings

    public WorldBuilder WithDegreeOfParallelism(int degreeOfParallelism)
    {
        _degreeOfParallelism = degreeOfParallelism;

        return this;
    }

    public WorldBuilder WithInitialCapacity(int initialCapacity)
    {
        _initialCapacity = initialCapacity;

        return this;
    }

    #endregion

    public World Build()
    {
        var world = new World(
            initialCapacity: _initialCapacity ?? 128,
            degreeOfParallelism: _degreeOfParallelism ?? Environment.ProcessorCount);

        world.LoadSystems(
            _drawSystems.Select(entry => entry.Invoke(world)),
            _updateSystems.Select(entry => entry.Invoke(world)));

        _drawSystems.Clear();
        _drawSystems.Clear();

        return world;
    }

    private readonly struct Entry<TResult>
        where TResult : class
    {
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _builder == null && _instance == null;
        }

        private readonly Func<World, TResult> _builder;
        private readonly TResult? _instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entry(TResult instance)
        {
            _builder = null!;
            _instance = instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entry(Func<World, TResult> builder)
        {
            _builder = builder;
            _instance = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Invoke(World context)
        {
            return _instance ?? _builder(context);
        }
    }
}
