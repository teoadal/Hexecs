namespace Hexecsm.Events;

internal interface IProducer : IDisposable;

internal interface IProducer<TMessage> : IProducer
    where TMessage : struct, IMessage
{
    void Produce(in TMessage message);

    void Produce(ReadOnlySpan<TMessage> messages);
}
