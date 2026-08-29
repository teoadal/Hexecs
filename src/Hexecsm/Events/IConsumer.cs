namespace Hexecsm.Events;

public interface IConsumer<TMessage>
    where TMessage : struct, IMessage
{
    void Handle(ReadOnlySpan<TMessage> message);
}
