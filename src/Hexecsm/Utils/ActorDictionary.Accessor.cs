namespace Hexecsm.Utils;

internal sealed partial class ActorDictionary<TValue>
{
    public readonly ref struct Accessor
    {
        public Span<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values;
        }

        private readonly uint[] _mapping;
        private readonly Span<TValue> _values;

        [SkipLocalsInit]
        internal Accessor(uint[] mapping, Span<TValue> values)
        {
            _mapping = mapping;
            _values = values;
        }

        public ref TValue this[ActorId actorId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                uint keyRaw = actorId.Value;

                if (keyRaw < (uint)_mapping.Length)
                {
                    uint denseIndexPlusOne = _mapping[keyRaw];

                    if (denseIndexPlusOne != 0)
                    {
                        int index = (int)denseIndexPlusOne - 1;

                        if (index < _values.Length)
                        {
                            return ref _values[index];
                        }
                    }
                }

                return ref Unsafe.NullRef<TValue>();
            }
        }
    }
}
