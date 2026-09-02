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
}
