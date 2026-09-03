using Hexecsm.Components;

namespace Hexecsm.Operations;

internal sealed partial class OperationQueue
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private readonly struct Operation
    {
        public readonly ActorId ActorId; // 4 байта (offset 0)
        public readonly ComponentTypeId ComponentId; // 2 байта (offset 4)
        public readonly int ComponentIndex; // 4 байта (offset 6)
        public readonly OperationType Type; // 1 байт  (offset 10)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public Operation(OperationType type, ActorId actorId)
        {
            ActorId = actorId;
            Type = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public Operation(OperationType type, ActorId actorId, ComponentTypeId componentId)
        {
            ActorId = actorId;
            ComponentId = componentId;
            Type = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public Operation(OperationType type, ActorId actorId, ComponentTypeId componentId, int componentIndex)
        {
            ActorId = actorId;
            ComponentId = componentId;
            ComponentIndex = componentIndex;
            Type = type;
        }
    }

    private enum OperationType : byte
    {
        ActorAdd,
        ActorRemove,
        ComponentAdd,
        ComponentClone,
        ComponentRemove,
        ComponentUpdate,
    }
}
