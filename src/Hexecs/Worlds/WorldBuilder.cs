using Hexecs.Actors;
using Hexecs.Assets.Sources;
using Hexecs.Configurations;
using Hexecs.Dependencies;
using Hexecs.Loggers;
using Hexecs.Threading;
using Hexecs.Values;

namespace Hexecs.Worlds;

public sealed class WorldBuilder : IDependencyCollection
{
    private readonly List<Func<World, IAssetSource>> _assetSourceBuilders = new(4);
    private readonly List<IDependencyRegistrar> _registrars = [];
    private readonly List<Dependency> _dependencies = [];

    private ConfigurationService? _configurationService;
    private LogService? _logService;

    private Action<ActorContextBuilder>? _defaultActorContextBuilder;
    private bool _debugWorld;
    private ValueService? _valueStorage;

    public WorldBuilder Add(DependencyLifetime lifetime, Type contract, Func<IDependencyProvider, object> resolver)
    {
        return lifetime switch
        {
            DependencyLifetime.Singleton => Singleton(contract, resolver),
            DependencyLifetime.Scoped => Scoped(contract, resolver),
            DependencyLifetime.Transient => Transient(contract, resolver),
            _ => DependencyError.NotSupportedLifetime<WorldBuilder>(lifetime)
        };
    }

    public WorldBuilder AddRegistrar(IDependencyRegistrar registrar)
    {
        _registrars.Add(registrar);
        return this;
    }


    #region AssetSource

    public WorldBuilder AddAssetSource(IAssetSource source)
    {
        _assetSourceBuilders.Add(_ => source);
        return this;
    }

    public WorldBuilder AddAssetSource<T>() where T : class, IAssetSource, new()
    {
        _assetSourceBuilders.Add(static _ => new T());
        return this;
    }

    public WorldBuilder CreateAssetSource(Func<World, IAssetSource> source)
    {
        _assetSourceBuilders.Add(source);
        return this;
    }

    public WorldBuilder CreateAssetData(int order, Action<IAssetLoader> source)
    {
        _assetSourceBuilders.Add(_ => new ActionAssetLoader(order, source));
        return this;
    }

    public WorldBuilder CreateAssetData(Action<IAssetLoader> source) => CreateAssetData(int.MaxValue, source);

    #endregion

    public World Build()
    {
        foreach (var registrar in _registrars)
        {
            registrar.TryRegister(this);
        }

        _registrars.Clear();

        var dependencyProvider = new DependencyProvider(_dependencies
            .GroupBy(static dep => dep.Contract)
            .Select(static group => new KeyValuePair<Type, Dependency[]>(group.Key, group.ToArray()))
            .ToDictionary(ReferenceComparer<Type>.Instance), null);

        var instance = new World(
            _configurationService ?? ConfigurationService.Empty,
            _logService ?? LogService.Empty,
            dependencyProvider,
            _defaultActorContextBuilder ?? DelegateUtils<ActorContextBuilder>.EmptyAction,
            _valueStorage ?? ValueService.Empty);

        if (_debugWorld) WorldDebug.World = instance;

        LoadAssets(instance);

        _dependencies.Clear();

        return instance;
    }

    public WorldBuilder CreateConfiguration(Action<ConfigurationBuilder> configuration)
    {
        var builder = new ConfigurationBuilder();
        configuration(builder);

        _configurationService = builder.Build();

        return this;
    }

    public WorldBuilder CreateLogger(Action<LogBuilder> logger)
    {
        var builder = new LogBuilder();
        logger(builder);

        _logService = builder.Build();

        return this;
    }

    public WorldBuilder CreateValues(Action<ValueServiceBuilder> values)
    {
        var builder = new ValueServiceBuilder();
        values(builder);

        _valueStorage = builder.Build();

        return this;
    }

    public WorldBuilder DebugWorld(bool value = true)
    {
        _debugWorld = value;
        return this;
    }

    public WorldBuilder DefaultActorContext(Action<ActorContextBuilder> defaultActorContext)
    {
        _defaultActorContextBuilder = defaultActorContext;
        return this;
    }

    public WorldBuilder DefaultParallelWorker(int? degreeOfParallelism = null)
    {
        degreeOfParallelism ??= Environment.ProcessorCount;
        return DefaultParallelWorker(new DefaultParallelWorker(degreeOfParallelism.Value));
    }

    public WorldBuilder DefaultParallelWorker(IParallelWorker worker)
    {
        return Singleton(typeof(IParallelWorker), _ => worker);
    }

    public WorldBuilder Singleton(Type contract, Func<IDependencyProvider, object> resolver)
    {
        _dependencies.Add(new Dependency(DependencyLifetime.Singleton, contract, resolver));
        return this;
    }

    public WorldBuilder Singleton<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return Singleton(typeof(T), resolver);
    }

    public WorldBuilder Scoped(Type contract, Func<IDependencyProvider, object> resolver)
    {
        _dependencies.Add(new Dependency(DependencyLifetime.Scoped, contract, resolver));
        return this;
    }

    public WorldBuilder Scoped<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return Scoped(typeof(T), resolver);
    }

    public WorldBuilder Transient(Type contract, Func<IDependencyProvider, object> resolver)
    {
        _dependencies.Add(new Dependency(DependencyLifetime.Transient, contract, resolver));
        return this;
    }

    public WorldBuilder Transient<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return Transient(typeof(T), resolver);
    }

    private void LoadAssets(World world)
    {
        var sortedAssetSources = _assetSourceBuilders
            .Select(func => func(world))
            .Order(OrderComparer<IAssetSource>.CreateInstance());

        world.Assets.LoadAssets(sortedAssetSources);

        _assetSourceBuilders.Clear();
    }

    #region Interfaces

    IDependencyCollection IDependencyCollection.AddRegistrar(IDependencyRegistrar registrar)
    {
        return AddRegistrar(registrar);
    }

    IDependencyCollection IDependencyCollection.Add(
        DependencyLifetime lifetime,
        Type contract,
        Func<IDependencyProvider, object> resolver)
    {
        return Add(lifetime, contract, resolver);
    }

    IDependencyCollection IDependencyCollection.Singleton(Type contract, Func<IDependencyProvider, object> resolver)
    {
        return Singleton(contract, resolver);
    }

    IDependencyCollection IDependencyCollection.Singleton<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return Singleton(resolver);
    }

    IDependencyCollection IDependencyCollection.Scoped(Type contract, Func<IDependencyProvider, object> resolver)
    {
        return Scoped(contract, resolver);
    }

    IDependencyCollection IDependencyCollection.Scoped<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return Scoped(resolver);
    }

    IDependencyCollection IDependencyCollection.Transient(Type contract, Func<IDependencyProvider, object> resolver)
    {
        return Transient(contract, resolver);
    }

    IDependencyCollection IDependencyCollection.Transient<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return Transient(resolver);
    }

    #endregion
}