using System.Collections;
using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal abstract partial class ArchetypeBase<TEntry>
{
    protected struct AccessEnumerator<T1> :
        IEnumerable<ArchetypeAccessor<T1>>,
        IEnumerator<ArchetypeAccessor<T1>>
        where T1 : struct, IActorComponent
    {
        public readonly ArchetypeAccessor<T1> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enumerator.GetCurrentAccessor<T1>();
        }

        private EntryEnumerator _enumerator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal AccessEnumerator(ArchetypeBase<TEntry> archetype)
        {
            _enumerator = archetype.GetEntryEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => _enumerator.MoveNext();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly AccessEnumerator<T1> GetEnumerator() => this;

        #region Interfaces

        readonly IEnumerator<ArchetypeAccessor<T1>> IEnumerable<ArchetypeAccessor<T1>>.GetEnumerator() => this;

        readonly IEnumerator IEnumerable.GetEnumerator() => this;

        object? IEnumerator.Current => null;

        readonly void IDisposable.Dispose()
        {
        }

        readonly void IEnumerator.Reset()
        {
        }

        #endregion
    }
}