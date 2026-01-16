using System.Text.Json;
using Hexecs.Serializations;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    [DebuggerDisplay("Length = {Length}")]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private struct Entry()
    {
        private const int InlineArraySize = 6;

        private InlineItemArray _inlineArray;
        private int _length = 0;
        private ushort[]? _array;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        public void Add(ushort item)
        {
            if (_length < InlineArraySize) _inlineArray[_length] = item;
            else
            {
                if (_array == null) _array = ArrayPool<ushort>.Shared.Rent(InlineArraySize);
                else ArrayUtils.Insert(ref _array, ArrayPool<ushort>.Shared, _length - InlineArraySize, item);
            }

            _length++;
        }

        public void Dispose()
        {
            if (_array is { Length: > 0 }) ArrayPool<ushort>.Shared.Return(_array);
            _array = null;
            _length = 0;
        }

        public readonly EntryComponentEnumerator GetEnumerator()
        {
            ref var inlineRef = ref Unsafe.AsRef(in _inlineArray);
            ref var reference = ref Unsafe.As<InlineItemArray, ushort>(ref inlineRef);
            var span = MemoryMarshal.CreateSpan(ref reference, InlineArraySize);
            return new EntryComponentEnumerator(span, _array ?? [], _length);
        }

        public readonly int IndexOf(ushort item)
        {
            if (_length == 0) return -1;

            var inlineLength = Math.Min(_length, InlineArraySize);
            for (var i = 0; i < inlineLength; i++)
            {
                if (_inlineArray[i] == item)
                {
                    return i;
                }
            }

            if (_array == null || _array.Length == 0) return -1;

            var span = _array.AsSpan(0, _length - InlineArraySize);
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == item) return InlineArraySize + i;
            }

            return -1;
        }

        public bool Remove(ushort item)
        {
            if (_length == 0) return false;

            var arraySize = _length - InlineArraySize;
            var inlineLength = Math.Min(_length, InlineArraySize);

            for (var index = 0; index < inlineLength; index++)
            {
                if (_inlineArray[index] != item) continue;

                var inlineEnd = inlineLength - 1;
                for (var i = index; i < inlineEnd; i++)
                {
                    _inlineArray[i] = _inlineArray[i + 1];
                }

                if (arraySize > 0)
                {
                    const int lastInlineIndex = InlineArraySize - 1;
                    _inlineArray[lastInlineIndex] = _array![0];
                    ArrayUtils.Cut(_array, 0, arraySize);
                }

                _length--;
                return true;
            }

            if (_array == null || _array.Length == 0 || arraySize <= 0) return false;

            var span = _array.AsSpan(0, arraySize);
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] != item) continue;
                ArrayUtils.Cut(_array, i, arraySize);
                _length--;

                return true;
            }

            return false;
        }

        public readonly void Serialize(uint actorId, Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteProperty("Key", actorId);
            writer.WritePropertyName("Components");

            writer.WriteStartArray();
            foreach (var component in this)
            {
                writer.WriteNumberValue(component);
            }

            writer.WriteStartArray();

            writer.WriteEndObject();
        }

        public readonly ushort this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => index < InlineArraySize
                ? _inlineArray[index]
                : _array![index - InlineArraySize];
        }

        public readonly ushort[] ToArray()
        {
            if (_length == 0) return [];

            var result = ArrayUtils.Create<ushort>(_length);
            for (var i = 0; i < _length; i++)
            {
                result[i] = this[i];
            }

            return result;
        }

        public bool TryAdd(ushort item)
        {
            var has = IndexOf(item) > -1;
            if (has) return false;

            Add(item);
            return true;
        }

        public ref struct EntryComponentEnumerator
        {
            public readonly ref ushort Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _index < InlineArraySize
                    ? ref _inlineArray[_index]
                    : ref _array[_index - InlineArraySize];
            }

            private readonly Span<ushort> _inlineArray;
            private readonly ushort[] _array;
            private readonly int _length;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal EntryComponentEnumerator(Span<ushort> inlineArray, ushort[] array, int length)
            {
                _inlineArray = inlineArray;
                _array = array;
                _length = length;
                _index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++_index < _length;
        }

        [InlineArray(InlineArraySize)]
        private struct InlineItemArray
        {
            private ushort _item;
        }
    }
}