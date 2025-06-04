using Hexecs.Assets.Components;
using Hexecs.Assets.Sources;

namespace Hexecs.Assets;

public sealed partial class AssetContext
{
    internal void LoadAssets(IEnumerable<IAssetSource> sources)
    {
        var loader = new Loader(this, _aliases);

        foreach (var source in sources)
        {
            source.Load(loader);

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (source is IDisposable disposable) disposable.Dispose();
        }

        loader.Dispose();
    }

    internal sealed class Loader : IAssetLoader, IDisposable
    {
        public readonly AssetContext Context;

        private readonly Dictionary<string, uint> _aliases;
        private readonly Dictionary<Type, object> _blockBuilders;
        private bool _disposed;

        private uint _nextId;

        // ReSharper disable once ConvertToPrimaryConstructor
        public Loader(AssetContext context, Dictionary<string, uint> aliases)
        {
            Context = context;

            _aliases = aliases;
            _blockBuilders = new Dictionary<Type, object>(ReferenceComparer<Type>.Instance);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AssetConfigurator CreateAsset()
        {
            var id = GetNextAssetId();
            return CreateAsset(id);
        }

        public AssetConfigurator CreateAsset<T1>(in T1 component1)
            where T1 : struct, IAssetComponent
        {
            var id = GetNextAssetId();
            return CreateAsset(id, in component1);
        }

        public AssetConfigurator CreateAsset<T1, T2>(in T1 component1, in T2 component2)
            where T1 : struct, IAssetComponent
            where T2 : struct, IAssetComponent
        {
            var id = GetNextAssetId();
            return CreateAsset(id, in component1, in component2);
        }

        public AssetConfigurator CreateAsset<T1, T2, T3>(in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IAssetComponent
            where T2 : struct, IAssetComponent
            where T3 : struct, IAssetComponent
        {
            var id = GetNextAssetId();
            return CreateAsset(id, in component1, in component2, in component3);
        }

        public AssetConfigurator CreateAsset(uint id)
        {
            EnsureNotDisposed();
            Context.AddEntry(id);
            return new AssetConfigurator(id, this);
        }

        public AssetConfigurator CreateAsset<T1>(uint id, in T1 component1)
            where T1 : struct, IAssetComponent
        {
            EnsureNotDisposed();
            ref var entry = ref Context.AddEntry(id);

            var pool1 = Context.GetOrCreateComponentPool<T1>();
            pool1.Set(id, in component1);
            entry.Add(AssetComponentType<T1>.Id);

            return new AssetConfigurator(id, this);
        }

        public AssetConfigurator CreateAsset<T1, T2>(uint id, in T1 component1, in T2 component2)
            where T1 : struct, IAssetComponent
            where T2 : struct, IAssetComponent
        {
            EnsureNotDisposed();
            ref var entry = ref Context.AddEntry(id);

            var pool1 = Context.GetOrCreateComponentPool<T1>();
            pool1.Set(id, in component1);
            entry.Add(AssetComponentType<T1>.Id);

            var pool2 = Context.GetOrCreateComponentPool<T2>();
            pool2.Set(id, in component2);
            entry.Add(AssetComponentType<T2>.Id);

            return new AssetConfigurator(id, this);
        }

        public AssetConfigurator CreateAsset<T1, T2, T3>(
            uint id,
            in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IAssetComponent
            where T2 : struct, IAssetComponent
            where T3 : struct, IAssetComponent
        {
            EnsureNotDisposed();
            ref var entry = ref Context.AddEntry(id);

            var pool1 = Context.GetOrCreateComponentPool<T1>();
            pool1.Set(id, in component1);
            entry.Add(AssetComponentType<T1>.Id);

            var pool2 = Context.GetOrCreateComponentPool<T2>();
            pool2.Set(id, in component2);
            entry.Add(AssetComponentType<T2>.Id);

            var pool3 = Context.GetOrCreateComponentPool<T3>();
            pool3.Set(id, in component3);
            entry.Add(AssetComponentType<T3>.Id);

            return new AssetConfigurator(id, this);
        }

        public AssetConfigurator CreateAsset(string alias)
        {
            var id = GetId(alias);
            return CreateAsset(id);
        }

        public AssetConfigurator CreateAsset<T1>(string alias, in T1 component1)
            where T1 : struct, IAssetComponent
        {
            var id = GetId(alias);
            return CreateAsset(id, in component1);
        }

        public AssetConfigurator CreateAsset<T1, T2>(string alias, in T1 component1, in T2 component2)
            where T1 : struct, IAssetComponent
            where T2 : struct, IAssetComponent
        {
            var id = GetId(alias);
            return CreateAsset(id, in component1, in component2);
        }

        public AssetConfigurator CreateAsset<T1, T2, T3>(
            string alias,
            in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IAssetComponent
            where T2 : struct, IAssetComponent
            where T3 : struct, IAssetComponent
        {
            var id = GetId(alias);
            return CreateAsset(id, in component1, in component2, in component3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNotDisposed()
        {
            if (_disposed) AssetError.LoaderDisposed();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Asset GetAsset(uint assetId) => Context.GetAsset(assetId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Asset GetAsset(string alias) => Context.GetAsset(GetId(alias));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Asset<T1> GetAsset<T1>(uint assetId)
            where T1 : struct, IAssetComponent => Context.GetAsset<T1>(assetId);

        public Asset<T1> GetAsset<T1>(string alias)
            where T1 : struct, IAssetComponent => Context.GetAsset<T1>(GetId(alias));

        public string GetAlias(uint assetId)
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var alias in _aliases)
            {
                if (alias.Value == assetId) return alias.Key;
            }

            AssetError.AliasNotFound(assetId);
            return string.Empty;
        }

        public uint GetId(string assetAlias)
        {
            if (_aliases.TryGetValue(assetAlias, out var exists)) return exists;

            var id = GetNextAssetId();
            _aliases.Add(assetAlias, id);
            return id;
        }

        public AssetBlockBuilder<TArray, TItem> RentBlockBuilder<TArray, TItem>(
            Func<ReadOnlyMemory<TItem>, TArray> blockBuilder)
            where TArray : struct, IAssetComponent, IArray<TItem>
            where TItem : struct
        {
            return _blockBuilders.Remove(typeof(AssetBlockBuilder<TArray, TItem>), out var exists)
                ? (AssetBlockBuilder<TArray, TItem>)exists
                : new AssetBlockBuilder<TArray, TItem>(this, blockBuilder);
        }

        public void ReturnBlockBuilder<TArray, TItem>(AssetBlockBuilder<TArray, TItem> builder)
            where TArray : struct, IAssetComponent, IArray<TItem>
            where TItem : struct
        {
            _blockBuilders.TryAdd(typeof(AssetBlockBuilder<TArray, TItem>), builder);
        }

        public ref T SetComponent<T>(uint assetId, in T component) where T : struct, IAssetComponent
        {
            EnsureNotDisposed();
            ref var entry = ref Context.GetEntryExact(assetId);

            var pool = Context.GetOrCreateComponentPool<T>();
            ref var reference = ref pool.Set(assetId, in component);

            entry.Add(AssetComponentType<T>.Id);

            return ref reference;
        }

        private uint GetNextAssetId()
        {
            EnsureNotDisposed();

            var id = Interlocked.Increment(ref _nextId);
            while (Context.ExistsAsset(id))
            {
                id = Interlocked.Increment(ref _nextId);
            }

            return id;
        }
    }
}