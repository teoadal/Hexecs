namespace Hexecsm.Events;

internal interface IProducer : IDisposable;

internal interface IProducer<in TMessage> : IProducer
    where TMessage : struct, IMessage
{
    void Produce(TMessage message);
}
