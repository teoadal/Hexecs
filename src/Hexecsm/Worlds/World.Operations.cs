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
}
