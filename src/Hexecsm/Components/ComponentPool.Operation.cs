namespace Hexecsm.Components;

internal sealed partial class ComponentPool<T>
{
    [DebuggerDisplay("Type = {Type}, ActorId = {ActorId}")]
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
