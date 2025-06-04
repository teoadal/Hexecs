using System.Text.Json;
using Hexecs.Serializations;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private struct Entry
    {
        public uint Key;
        public int Next;
        public ComponentBucket Components;

        public readonly void Serialize(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteProperty("Key", Key);
            //writer.WriteProperty("Components", in Components);

            writer.WriteEndObject();
        }
    }

    [DebuggerDisplay("Length = {Length}")]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal struct ComponentBucket()
    {
        public const int InlineArraySize = 10;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        private InlineItemArray _inlineArray;
        private ushort[] _array = [];
        private int _length = 0;

        public void Add(ushort item)
        {
            if (_length < InlineArraySize) _inlineArray[_length] = item;
            else ArrayUtils.Insert(ref _array, ArrayPool<ushort>.Shared, _length - InlineArraySize, item);

            _length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(ushort item) => IndexOf(item) > -1;

        public void Dispose()
        {
            if (_array is { Length: > 0 }) ArrayPool<ushort>.Shared.Return(_array);
            _array = [];
            _length = 0;
        }

        public ComponentBucketEnumerator GetEnumerator()
        {
            ref var reference = ref Unsafe.As<InlineItemArray, ushort>(ref _inlineArray);
            var span = MemoryMarshal.CreateSpan(ref reference, InlineArraySize);
            return new ComponentBucketEnumerator(span, _array, _length);
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
                    _inlineArray[lastInlineIndex] = _array[0];
                    ArrayUtils.Cut(_array, 0, arraySize);
                }

                _length--;
                return true;
            }

            if (_array.Length == 0 || arraySize <= 0) return false;

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

        public ushort this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => index < InlineArraySize ? _inlineArray[index] : _array[index - InlineArraySize];
            set
            {
                if (index < InlineArraySize) _inlineArray[index] = value;
                else _array[index - InlineArraySize] = value;
            }
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
            var has = Contains(item);
            if (has) return false;

            Add(item);
            return true;
        }

        public ref struct ComponentBucketEnumerator
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
            internal ComponentBucketEnumerator(Span<ushort> inlineArray, ushort[] array, int length)
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