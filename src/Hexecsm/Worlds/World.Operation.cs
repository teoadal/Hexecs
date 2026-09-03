using Hexecsm.Components;

namespace Hexecsm.Worlds;

public sealed partial class World
{
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

    private enum OperationType : byte
    {
        ActorAdd,
        ActorDestroy,
        ClearWorld,
        ComponentAdd,
        ComponentRemove,
    }
}
