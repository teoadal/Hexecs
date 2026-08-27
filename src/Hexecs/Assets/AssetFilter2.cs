using System.Collections.Frozen;
using Hexecs.Assets.Components;

namespace Hexecs.Assets;

[DebuggerTypeProxy(typeof(AssetFilter<,>.DebugProxy))]
[DebuggerDisplay("Length = {Length}")]
public sealed partial class AssetFilter<T1, T2> : IAssetFilter
    where T1 : struct, IAssetComponent
    where T2 : struct, IAssetComponent
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

    internal AssetFilter(AssetContext context, AssetConstraint? constraint = null)
    {
        Context = context;
        Constraint = constraint;

        _pool1 = context.GetOrCreateComponentPool<T1>();
        _pool2 = context.GetOrCreateComponentPool<T2>();
        _dictionary = Collect(context, _pool1, _pool2, constraint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(AssetId assetId)
    {
        return _dictionary.ContainsKey(assetId.Value);
    }

    public AssetRef<T1, T2> Get(AssetId assetId)
    {
        if (!_dictionary.TryGetValue(assetId.Value, out Entry entry))
        {
            AssetError.NotFound(assetId);
        }

        return new AssetRef<T1, T2>(
            Context,
            assetId,
            ref _pool1.GetByIndex(entry.Index1),
            ref _pool2.GetByIndex(entry.Index2));
    }

    public Asset[] ToArray()
    {
        FrozenDictionary<uint, Entry> dictionary = _dictionary;

        int count = dictionary.Count;
        if (count == 0)
        {
            return [];
        }

        var assets = new Asset[count];
        AssetContext ctx = Context;

        var index = 0;
        foreach (uint assetId in dictionary.Keys)
        {
            assets[index++] = new Asset(ctx, new AssetId(assetId));
        }

        return assets;
    }
    
    private static FrozenDictionary<uint, Entry> Collect(
        AssetContext context,
        AssetComponentPool<T1> pool1,
        AssetComponentPool<T2> pool2,
        AssetConstraint? constraint)
    {
        ArrayPool<KeyValuePair<uint, Entry>> bufferPool = ArrayPool<KeyValuePair<uint, Entry>>.Shared;
        KeyValuePair<uint, Entry>[] buffer = bufferPool.Rent(16);
        var length = 0;

        Func<AssetId, bool> constraintFunction = constraint == null
            ? DelegateUtils<AssetId>.AlwaysTrue
            : constraint.Applicable;

        foreach (Asset asset in context)
        {
            AssetId assetId = asset.Id;

            int index1 = pool1.TryGetIndex(assetId);
            if (index1 == -1)
            {
                continue;
            }

            int index2 = pool2.TryGetIndex(assetId);
            if (index2 == -1)
            {
                continue;
            }

            if (!constraintFunction(assetId))
            {
                continue;
            }

            ArrayUtils.Insert(
                ref buffer,
                bufferPool,
                length,
                new KeyValuePair<uint, Entry>(assetId.Value, new Entry(index1, index2)));

            length++;
        }

        var segment = new ArraySegment<KeyValuePair<uint, Entry>>(buffer, 0, length);
        FrozenDictionary<uint, Entry> result = segment.ToFrozenDictionary();

        bufferPool.Return(buffer, true);

        return result;
    }

    AssetContext IAssetFilter.Context => Context;
    AssetConstraint? IAssetFilter.Constraint => Constraint;
}