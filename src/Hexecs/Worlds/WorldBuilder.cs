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
            DependencyLifetime.Singleton => UseSingleton(contract, resolver),
            DependencyLifetime.Scoped => UseScoped(contract, resolver),
            DependencyLifetime.Transient => UseTransient(contract, resolver),
            _ => DependencyError.NotSupportedLifetime<WorldBuilder>(lifetime)
        };
    }

    public WorldBuilder AddRegistrar(IDependencyRegistrar registrar)
    {
        _registrars.Add(registrar);
        return this;
    }

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

    #region AssetSource

    public WorldBuilder UseAddAssetSource(IAssetSource source)
    {
        _assetSourceBuilders.Add(_ => source);
        return this;
    }

    public WorldBuilder UseAddAssetSource<T>() where T : class, IAssetSource, new()
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

    public WorldBuilder UseAsDebugWorld(bool value = true)
    {
        _debugWorld = value;
        return this;
    }

    public WorldBuilder UseConfiguration(Action<ConfigurationBuilder> configuration)
    {
        var builder = new ConfigurationBuilder();
        configuration(builder);

        _configurationService = builder.Build();

        return this;
    }

    public WorldBuilder UseDefaultActorContext(Action<ActorContextBuilder> defaultActorContext)
    {
        _defaultActorContextBuilder = defaultActorContext;
        return this;
    }

    #region ParallelWorker

    public WorldBuilder UseDefaultParallelWorker(int? degreeOfParallelism = null)
    {
        return UseDefaultParallelWorker(ctx =>
        {
            if (degreeOfParallelism == null)
            {
                var configuration = ctx.GetService<ConfigurationService>();
                degreeOfParallelism = configuration?.GetValue<int>("ParallelWorker:DegreeOfParallelism");
            }

            return new DefaultParallelWorker(degreeOfParallelism ?? Environment.ProcessorCount);
        });
    }

    public WorldBuilder UseDefaultParallelWorker(IParallelWorker worker)
    {
        return UseDefaultParallelWorker(_ => worker);
    }

    public WorldBuilder UseDefaultParallelWorker(Func<IDependencyProvider, IParallelWorker> worker)
    {
        return UseSingleton(worker);
    }

    #endregion

    public WorldBuilder UseLogger(Action<LogBuilder> logger)
    {
        var builder = new LogBuilder();
        logger(builder);

        _logService = builder.Build();

        return this;
    }

    public WorldBuilder UseValues(Action<ValueServiceBuilder> values)
    {
        var builder = new ValueServiceBuilder();
        values(builder);

        _valueStorage = builder.Build();

        return this;
    }

    #region Singleton

    public WorldBuilder UseSingleton<T>(T value)
        where T : class
    {
        _dependencies.Add(new Dependency(DependencyLifetime.Singleton, typeof(T), _ => value));
        return this;
    }


    public WorldBuilder UseSingleton(Type contract, Func<IDependencyProvider, object> resolver)
    {
        _dependencies.Add(new Dependency(DependencyLifetime.Singleton, contract, resolver));
        return this;
    }

    public WorldBuilder UseSingleton<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return UseSingleton(typeof(T), resolver);
    }

    #endregion

    #region Scoped

    public WorldBuilder UseScoped(Type contract, Func<IDependencyProvider, object> resolver)
    {
        _dependencies.Add(new Dependency(DependencyLifetime.Scoped, contract, resolver));
        return this;
    }

    public WorldBuilder UseScoped<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return UseScoped(typeof(T), resolver);
    }

    #endregion

    #region Transient

    public WorldBuilder UseTransient(Type contract, Func<IDependencyProvider, object> resolver)
    {
        _dependencies.Add(new Dependency(DependencyLifetime.Transient, contract, resolver));
        return this;
    }

    public WorldBuilder UseTransient<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return UseTransient(typeof(T), resolver);
    }

    #endregion

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

    IDependencyCollection IDependencyCollection.UseSingleton(Type contract, Func<IDependencyProvider, object> resolver)
    {
        return UseSingleton(contract, resolver);
    }

    IDependencyCollection IDependencyCollection.UseSingleton<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return UseSingleton(resolver);
    }

    IDependencyCollection IDependencyCollection.UseScoped(Type contract, Func<IDependencyProvider, object> resolver)
    {
        return UseScoped(contract, resolver);
    }

    IDependencyCollection IDependencyCollection.UseScoped<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return UseScoped(resolver);
    }

    IDependencyCollection IDependencyCollection.UseTransient(Type contract, Func<IDependencyProvider, object> resolver)
    {
        return UseTransient(contract, resolver);
    }

    IDependencyCollection IDependencyCollection.UseTransient<T>(Func<IDependencyProvider, T> resolver) where T : class
    {
        return UseTransient(resolver);
    }

    #endregion
}