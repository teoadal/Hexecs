namespace Hexecsm.Events;

internal interface IProducer;

internal interface IProducer<TMessage> : IProducer
    where TMessage : struct, IMessage
{
    void Produce(params ReadOnlySpan<TMessage> messages);
}
