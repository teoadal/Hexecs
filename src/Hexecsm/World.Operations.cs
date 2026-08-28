using System.Collections.Concurrent;

using Hexecsm.Components;

namespace Hexecsm;

public sealed partial class World
{
    private readonly ConcurrentQueue<Operation> _postponedOperations = [];

    private void PostponeOperation(in Operation operation)
    {
        _postponedOperations.Enqueue(operation);
    }

    private void ProcessPostponedOperations()
    {
        while (_postponedOperations.TryDequeue(out Operation operation))
        {
            switch (operation.Type)
            {
                case OperationType.AddActor:
                {
                    AddHandler(operation.ActorId);

                    break;
                }
                case OperationType.ClearWorld:
                {
                    ClearHandler();
                    _postponedOperations.Clear();

                    return;
                }
                case OperationType.DestroyActor:
                {
                    DestroyHandler(operation.ActorId);

                    break;
                }
            }
        }

        foreach (IComponentPool? componentPool in _componentPools)
        {
            componentPool?.ProcessPostponedOperations();
        }
    }

    private readonly struct Operation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Add(ActorId actorId)
        {
            return new Operation(actorId, OperationType.AddActor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Clear()
        {
            return new Operation(OperationType.ClearWorld);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Operation Destroy(ActorId actorId)
        {
            return new Operation(actorId, OperationType.DestroyActor);
        }

        public readonly ActorId ActorId;
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
    }

    private enum OperationType
    {
        AddActor,
        ClearWorld,
        DestroyActor
    }
}
