namespace Hexecsm.Events;

internal interface IConsumer<TMessage>
    where TMessage : struct, IMessage
{
    void Handle(in TMessage message);
}
