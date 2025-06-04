namespace Hexecs.Dependencies;

internal sealed class DependencyProvider : Dictionary<DependencyKey, object?>, IDependencyProvider, IDisposable
{
    private static List<object>? _arrayBuffer;

    private readonly Dictionary<Type, Dependency[]> _dependencies;
    private readonly List<IDisposable> _disposables;
    private readonly HashSet<Type> _progress;
    private readonly DependencyProvider? _root;

    private bool _disposed;

    public DependencyProvider(
        Dictionary<Type, Dependency[]> dependencies,
        DependencyProvider? root)
    {
        _arrayBuffer = new List<object>(8);
        _dependencies = dependencies;
        _disposables = new List<IDisposable>(8);
        _progress = new HashSet<Type>(8, ReferenceComparer<Type>.Instance);
        _root = root;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true; // self-disposing protection

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        Clear();

        _disposables.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TService? GetService<TService>() where TService : class
    {
        return GetService(DependencyKey.First(typeof(TService))) as TService;
    }

    public TService[] GetServices<TService>() where TService : class
    {
        EnsureNotDisposed();

        var collectionType = typeof(TService[]);
        var serviceType = typeof(TService);

        var collectionKey = DependencyKey.First(collectionType);
        if (TryGetValue(collectionKey, out var exists))
        {
            return exists == null
                ? []
                : (TService[])exists;
        }

        if (!_progress.Add(collectionType)) DependencyError.CircularDependency(serviceType, _progress);

        var buffer = RentArrayBuffer();
        var storeArray = true;

        if (_dependencies.TryGetValue(serviceType, out var dependencies))
        {
            for (var i = 0; i < dependencies.Length; i++)
            {
                var key = new DependencyKey(serviceType, i);
                if (TryGetValue(key, out var resolved) && resolved != null)
                {
                    buffer.Add(resolved);
                    continue;
                }

                var dependency = dependencies[i];
                var instance = ResolveInstance(in dependency);

                if (instance == null) TryAdd(key, null);
                else
                {
                    var isCached = TryCache(dependency.Lifetime, in key, instance);
                    if (storeArray) storeArray = isCached;
                }
            }
        }

        var array = buffer.Cast<TService>().ToArray();

        ReturnArrayBuffer(buffer);

        if (storeArray) TryAdd(collectionKey, array);

        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetService(Type contract) => GetService(DependencyKey.First(contract));

    public DependencyProvider GetScopeProvider()
    {
        EnsureNotDisposed();

        var provider = new DependencyProvider(_dependencies, _root ?? this);
        provider.Add(DependencyKey.First(typeof(IDependencyProvider)), provider);
        return provider;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<object> RentArrayBuffer(int capacity = 8)
    {
        return Interlocked.Exchange(ref _arrayBuffer, null) ?? new List<object>(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReturnArrayBuffer(List<object> buffer)
    {
        buffer.Clear();
        Interlocked.Exchange(ref _arrayBuffer, buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object? ResolveInstance(in Dependency dependency)
    {
        if (dependency.Lifetime == DependencyLifetime.Singleton)
        {
            return _root == null
                ? dependency.Resolve(this)
                : _root.GetService(dependency.Contract);
        }

        return dependency.Resolve(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureNotDisposed()
    {
        if (_disposed) DependencyError.Disposed();
    }

    private object? GetService(in DependencyKey key)
    {
        EnsureNotDisposed();

        var contract = key.ServiceType;

        if (TryGetValue(key, out var exists)) return exists;
        if (!_progress.Add(contract)) DependencyError.CircularDependency(contract, _progress);

        object? instance;

        if (_dependencies.TryGetValue(contract, out var dependencies))
        {
            var dependency = dependencies[key.Index];
            instance = ResolveInstance(in dependency);

            if (instance == null) TryAdd(key, null);
            else TryCache(dependency.Lifetime, in key, instance);
        }
        else
        {
            instance = _root?.GetService(contract);
            TryAdd(key, instance);
        }

        _progress.Remove(contract);

        return instance;
    }

    private bool TryCache(DependencyLifetime lifetime, in DependencyKey key, object instance)
    {
        var isCached = false;
        if (lifetime != DependencyLifetime.Transient)
        {
            TryAdd(key, instance);
            isCached = true;
        }

        if (instance is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        return isCached;
    }
}