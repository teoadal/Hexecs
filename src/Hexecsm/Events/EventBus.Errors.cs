namespace Hexecsm.Events;

internal sealed partial class EventBus
{
    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowConsumerAlreadyRegistered()
    {
        throw new InvalidOperationException("Consumer already registered");
    }

    [StackTraceHidden]
    [DoesNotReturn]
    private static void ThrowConsumerNotRegistered()
    {
        throw new InvalidOperationException("Consumer not registered");
    }
}
