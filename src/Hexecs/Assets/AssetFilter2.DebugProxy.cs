namespace Hexecs.Assets;

public sealed partial class AssetFilter<T1, T2>
{
    private sealed class DebugProxy(AssetFilter<T1, T2> filter)
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<Asset> Assets
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => filter._dictionary.Keys.Select(key => new Asset(filter.Context, key));
        }
    }
}