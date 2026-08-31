using Hexecsm.Utils;

namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    private readonly ThreadLocalQueue<Operation> _postponedOperations = new ThreadLocalQueue<Operation>(128);

    public void ProcessPostponedOperations()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ComponentPool<T>));

        foreach (ref ThreadLocalQueue<Operation>.LocalQueue batch in _postponedOperations.GetBatches())
        {
            foreach (ref Operation operation in batch.AsSpan())
            {
                ExecuteOperation(in operation);
            }

            batch.Clear();
        }
    }

    private void ExecuteOperation(in Operation operation)
    {
        ActorId actorId = operation.ActorId;

        switch (operation.Type)
        {
            case OperationType.Add:
            {
                AddHandler(actorId, in operation.Component);

                break;
            }
            case OperationType.Clone:
            {
                CloneHandler(actorId, in operation.Component);

                break;
            }
            case OperationType.Remove:
            {
                RemoveHandler(actorId);

                break;
            }
            case OperationType.Update:
            {
                UpdateHandler(actorId, in operation.Component);

                break;
            }
            default:
                ThrowInvalidOperation(actorId, operation.Type);

                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PostponeOperation(in Operation operation)
    {
        _postponedOperations.Enqueue(operation);
    }

    private readonly struct Operation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(in ActorId actorId, in T component)
        {
            return new Operation(actorId, in component, OperationType.Add);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Clone(ActorId target, in T component)
        {
            return new Operation(target, in component, OperationType.Clone);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(in ActorId actorId)
        {
            return new Operation(actorId, OperationType.Remove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Remove(in ActorId actorId, in T component)
        {
            return new Operation(actorId, in component, OperationType.Remove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Update(in ActorId actorId, in T component)
        {
            return new Operation(actorId, in component, OperationType.Update);
        }

        public readonly ActorId ActorId;
        public readonly OperationType Type;
        public readonly T Component;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        private Operation(ActorId actorId, OperationType type)
        {
            ActorId = actorId;
            Type = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        private Operation(ActorId actorId, in T component, OperationType type)
        {
            ActorId = actorId;
            Type = type;
            Component = component;
        }
    }

    private enum OperationType : byte
    {
        Add,
        Clone,
        Remove,
        Update
    }
}
