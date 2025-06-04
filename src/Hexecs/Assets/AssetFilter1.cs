using System.Collections.Frozen;
using Hexecs.Assets.Components;

namespace Hexecs.Assets;

[DebuggerTypeProxy(typeof(AssetFilter<>.DebugProxy))]
[DebuggerDisplay("Length = {Length}")]
public sealed partial class AssetFilter<T1> : IAssetFilter
    where T1 : struct, IAssetComponent
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

    internal AssetFilter(AssetContext context, AssetConstraint? constraint = null)
    {
        Context = context;
        Constraint = constraint;

        _pool1 = context.GetOrCreateComponentPool<T1>();
        _dictionary = Collect(context, _pool1, constraint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(uint assetId) => _dictionary.ContainsKey(assetId);

    public AssetRef<T1> Get(uint assetId)
    {
        if (!_dictionary.TryGetValue(assetId, out var entry)) AssetError.NotFound(assetId);

        return new AssetRef<T1>(
            Context,
            assetId,
            ref _pool1.GetByIndex(entry.Index1));
    }

    private static FrozenDictionary<uint, Entry> Collect(
        AssetContext context,
        AssetComponentPool<T1> pool1,
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

            if (!constraintFunction(assetId)) continue;

            ArrayUtils.Insert(
                ref buffer,
                bufferPool,
                length,
                new KeyValuePair<uint, Entry>(assetId, new Entry(index1)));

            length++;
        }

        var result = buffer.ToFrozenDictionary();

        bufferPool.Return(buffer, true);

        return result;
    }

    AssetContext IAssetFilter.Context => Context;
    AssetConstraint? IAssetFilter.Constraint => Constraint;
}