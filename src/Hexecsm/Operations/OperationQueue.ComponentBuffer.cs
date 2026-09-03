using Hexecsm.Components;

namespace Hexecsm.Operations;

internal sealed partial class OperationQueue
{
    private IComponentBuffer?[] _deferredBuffers = new IComponentBuffer?[128];
    private readonly Lock _deferredBuffersLock = new Lock();

    private interface IComponentBuffer
    {
        void ProcessAdd(ActorId actorId, int index);
    }

    private sealed class ComponentBuffer<T>(int initialCapacity, ComponentPool<T> componentPool) : IComponentBuffer
        where T : struct, IComponent
    {
        public readonly ComponentPool<T> Pool = componentPool;

        private readonly Lock _lock = new Lock();

        private T[] _buffer = new T[initialCapacity];
        private int _count;

        public int Add(in T component)
        {
            int index = Interlocked.Increment(ref _count);

            if ((uint)index < (uint)_buffer.Length)
            {
                _buffer[index] = component;

                return index;
            }

            return AddSlow(index, in component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessAdd(ActorId actorId, int index)
        {
            ref T component = ref _buffer[index];
            Pool.Add(actorId, in component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessRemove(ActorId actorId)
        {
            Pool.Remove(actorId);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int AddSlow(int index, in T component)
        {
            using (_lock.EnterScope())
            {
                if ((uint)index < (uint)_buffer.Length) // double check
                {
                    _buffer[index] = component;

                    return index;
                }

                Array.Resize(ref _buffer, _buffer.Length * 2);

                _buffer[index] = component;

                return index;
            }
        }
    }
}
