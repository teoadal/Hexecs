using System.Collections;
using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal abstract partial class ArchetypeBase<TEntry>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected EntryEnumerator GetEntryEnumerator()
    {
        return new EntryEnumerator(this);
    }

    protected struct EntryEnumerator : IEnumerable<TEntry>, IEnumerator<TEntry>
    {
        public readonly ref TEntry Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _values[_index];
        }

        private readonly TEntry[] _values;

        private readonly int _count;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EntryEnumerator(ArchetypeBase<TEntry> archetype)
        {
            _values = archetype._values;
            _count = archetype._count;
            _index = -1;
        }

        public readonly ArchetypeAccessor<T1> GetCurrentAccessor<T1>()
            where T1 : struct, IActorComponent
        {
            ref var entry = ref _values[_index];
            return new ArchetypeAccessor<T1>(
                TEntry.GetId(ref entry),
                ref TEntry.TryGetRef<T1>(ref entry));
        }

        public readonly ArchetypeAccessor<T1, T2> GetCurrentAccessor<T1, T2>()
            where T1 : struct, IActorComponent
            where T2 : struct, IActorComponent
        {
            ref var entry = ref _values[_index];
            return new ArchetypeAccessor<T1, T2>(
                TEntry.GetId(ref entry),
                ref TEntry.TryGetRef<T1>(ref entry),
                ref TEntry.TryGetRef<T2>(ref entry));
        }

        public readonly ArchetypeAccessor<T1, T2, T3> GetCurrentAccessor<T1, T2, T3>()
            where T1 : struct, IActorComponent
            where T2 : struct, IActorComponent
            where T3 : struct, IActorComponent
        {
            ref var entry = ref _values[_index];
            return new ArchetypeAccessor<T1, T2, T3>(
                TEntry.GetId(ref entry),
                ref TEntry.TryGetRef<T1>(ref entry),
                ref TEntry.TryGetRef<T2>(ref entry),
                ref TEntry.TryGetRef<T3>(ref entry));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly EntryEnumerator GetEnumerator() => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => ++_index < _count;

        #region Interfaces

        readonly IEnumerator<TEntry> IEnumerable<TEntry>.GetEnumerator() => this;

        readonly IEnumerator IEnumerable.GetEnumerator() => this;

        readonly TEntry IEnumerator<TEntry>.Current => Current;

        readonly object? IEnumerator.Current => null;

        readonly void IDisposable.Dispose()
        {
        }

        readonly void IEnumerator.Reset()
        {
        }

        #endregion
    }
}