using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private struct Entry
    {
        private InlineBucket<ComponentTypeId> _bucket;

        [SkipLocalsInit]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entry()
        {
            _bucket = new InlineBucket<ComponentTypeId>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _bucket.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(ComponentTypeId componentTypeId)
        {
            return _bucket.Remove(componentTypeId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(ComponentTypeId componentTypeId)
        {
            return _bucket.TryAdd(componentTypeId);
        }
    }
}
