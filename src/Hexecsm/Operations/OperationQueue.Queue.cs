using Hexecsm.Utils;

namespace Hexecsm.Operations;

internal sealed partial class OperationQueue
{
    private readonly MpscBlockQueue<Operation> _queue = new MpscBlockQueue<Operation>(1024);

    private void Enqueue(in Operation operation)
    {
        _queue.Enqueue(in operation);
    }

    private void Execute()
    {
        foreach (ReadOnlySpan<Operation> block in _queue)
        {
            foreach (Operation operation in block)
            {
                switch (operation.Type)
                {
                    case OperationType.ActorAdd:
                        break;
                    case OperationType.ActorRemove:
                        break;
                    case OperationType.ComponentAdd:
                    {
                        IComponentBuffer buffer = _componentBufferManager.GetUnsafe(operation.ComponentId);
                        buffer.ProcessAdd(operation.ActorId, operation.ComponentIndex);
                        break;
                    }
                    case OperationType.ComponentClone:
                        break;
                    case OperationType.ComponentRemove:
                    {
                        IComponentBuffer buffer = _componentBufferManager.GetUnsafe(operation.ComponentId);
                        buffer.ProcessAdd(operation.ActorId, operation.ComponentIndex);
                        break;
                    }
                    case OperationType.ComponentUpdate:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
