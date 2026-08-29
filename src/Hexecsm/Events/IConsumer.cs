namespace Hexecsm.Events;

internal interface IConsumer<in TMessage>
    where TMessage : struct, IMessage
{
    void Handle(TMessage message);
}
