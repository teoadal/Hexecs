using System.Collections.Frozen;
using Hexecs.Assets.Components;

namespace Hexecs.Assets;

[DebuggerTypeProxy(typeof(AssetFilter<,,>.DebugProxy))]
[DebuggerDisplay("Length = {Length}")]
public sealed partial class AssetFilter<T1, T2, T3> : IAssetFilter
    where T1 : struct, IAssetComponent
    where T2 : struct, IAssetComponent
    where T3 : struct, IAssetComponent
{
    public readonly AssetContext Context;
    public readonly AssetConstraint? Constraint;

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _dictionary.Count;
    }

    private readonly FrozenDictionary<uint, Entry> _dictionary;
    private readonly AssetComponentPool<T1> _pool1;
    private readonly AssetComponentPool<T2> _pool2;
    private readonly AssetComponentPool<T3> _pool3;

    internal AssetFilter(AssetContext context, AssetConstraint? constraint = null)
    {
        Context = context;
        Constraint = constraint;

        _pool1 = context.GetOrCreateComponentPool<T1>();
        _pool2 = context.GetOrCreateComponentPool<T2>();
        _pool3 = context.GetOrCreateComponentPool<T3>();
        _dictionary = Collect(context, _pool1, _pool2, _pool3, constraint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(uint assetId) => _dictionary.ContainsKey(assetId);

    public AssetRef<T1, T2, T3> Get(uint assetId)
    {
        if (!_dictionary.TryGetValue(assetId, out var entry)) AssetError.NotFound(assetId);

        return new AssetRef<T1, T2, T3>(
            Context,
            assetId,
            ref _pool1.GetByIndex(entry.Index1),
            ref _pool2.GetByIndex(entry.Index2),
            ref _pool3.GetByIndex(entry.Index3));
    }

    public Asset[] ToArray()
    {
        var dictionary = _dictionary;

        var count = dictionary.Count;
        if (count == 0) return [];

        var assets = new Asset[count];
        var ctx = Context;

        var index = 0;
        foreach (var assetId in dictionary.Keys)
        {
            assets[index++] = new Asset(ctx, assetId);
        }

        return assets;
    }

    private static FrozenDictionary<uint, Entry> Collect(
        AssetContext context,
        AssetComponentPool<T1> pool1,
        AssetComponentPool<T2> pool2,
        AssetComponentPool<T3> pool3,
        AssetConstraint? constraint)
    {
        var bufferPool = ArrayPool<KeyValuePair<uint, Entry>>.Shared;
        var buffer = bufferPool.Rent(16);
        var length = 0;

        var constraintFunction = constraint == null
            ? DelegateUtils<uint>.AlwaysTrue
            : constraint.Applicable;

        foreach (var asset in context)
        {
            var assetId = asset.Id;

            var index1 = pool1.TryGetIndex(assetId);
            if (index1 == -1) continue;

            var index2 = pool2.TryGetIndex(assetId);
            if (index2 == -1) continue;

            var index3 = pool3.TryGetIndex(assetId);
            if (index3 == -1) continue;

            if (!constraintFunction(assetId)) continue;

            ArrayUtils.Insert(
                ref buffer,
                bufferPool,
                length,
                new KeyValuePair<uint, Entry>(assetId, new Entry(index1, index2, index3)));

            length++;
        }

        var segment = new ArraySegment<KeyValuePair<uint, Entry>>(buffer, 0, length);
        var result = segment.ToFrozenDictionary();

        bufferPool.Return(buffer, true);

        return result;
    }

    AssetContext IAssetFilter.Context => Context;
    AssetConstraint? IAssetFilter.Constraint => Constraint;

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly struct Entry(int index1, int index2, int index3)
    {
        public readonly int Index1 = index1;
        public readonly int Index2 = index2;
        public readonly int Index3 = index3;
    }
}