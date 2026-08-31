using Hexecsm.Components;
using Hexecsm.Utils;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private readonly ThreadLocalQueue<Operation> _postponedOperations = new ThreadLocalQueue<Operation>(128);

    private void ExecuteOperation(in Operation operation)
    {
        switch (operation.Type)
        {
            case OperationType.ActorAdd:
            {
                ActorAddHandler(operation.ActorId);

                break;
            }
            case OperationType.ActorDestroy:
            {
                ActorDestroyHandler(operation.ActorId);

                break;
            }
            case OperationType.ComponentAdd:
            {
                ComponentAddHandler(operation.ActorId, operation.ComponentTypeId);

                break;
            }
            case OperationType.ComponentRemove:
            {
                ComponentRemoveHandler(operation.ActorId, operation.ComponentTypeId);

                break;
            }
            case OperationType.ClearWorld:
            {
                ClearHandler();

                return;
            }
            default:
            {
                ThrowInvalidOperation(operation.ActorId, operation.Type);

                break;
            }
        }
    }

    private void PostponeOperation(in Operation operation)
    {
        _postponedOperations.Enqueue(operation);
    }

    private void ProcessPostponedOperations()
    {
        foreach (ref ThreadLocalQueue<Operation>.LocalQueue batch in _postponedOperations.GetBatches())
        {
            foreach (ref Operation operation in batch.AsSpan())
            {
                ExecuteOperation(in operation);

                if (operation.Type != OperationType.ClearWorld)
                {
                    continue;
                }

                return;
            }

            batch.Clear();
        }
    }

    [DebuggerDisplay("Type = {Type}, ActorId = {ActorId}")]
    private readonly struct Operation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation AddActor(ActorId actorId)
        {
            return new Operation(actorId, OperationType.ActorAdd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation AddComponent(ActorId actorId, ComponentTypeId componentTypeId)
        {
            return new Operation(actorId, componentTypeId, OperationType.ComponentAdd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Clear()
        {
            return new Operation(OperationType.ClearWorld);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation DestroyActor(ActorId actorId)
        {
            return new Operation(actorId, OperationType.ActorDestroy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation RemoveComponent(ActorId actorId, ComponentTypeId componentTypeId)
        {
            return new Operation(actorId, componentTypeId, OperationType.ComponentRemove);
        }

        public readonly ActorId ActorId;
        public readonly ComponentTypeId ComponentTypeId;
        public readonly OperationType Type;

        [SkipLocalsInit]
        private Operation(OperationType type)
        {
            Type = type;
        }

        [SkipLocalsInit]
        private Operation(ActorId actorId, OperationType type)
        {
            ActorId = actorId;
            Type = type;
        }

        [SkipLocalsInit]
        private Operation(ActorId actorId, ComponentTypeId componentTypeId, OperationType type)
        {
            ActorId = actorId;
            ComponentTypeId = componentTypeId;
            Type = type;
        }
    }

    private enum OperationType
    {
        ActorAdd,
        ActorDestroy,
        ClearWorld,
        ComponentAdd,
        ComponentRemove,
    }
}
