using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    [method: SkipLocalsInit]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private struct Entry()
    {
        private InlineBucket<ComponentTypeId> _bucket = new InlineBucket<ComponentTypeId>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _bucket.Dispose();
        }

        public InlineBucket<ComponentTypeId>.Enumerator GetEnumerator()
        {
            return _bucket.GetEnumerator();
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
