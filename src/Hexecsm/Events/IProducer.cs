namespace Hexecsm.Events;

internal interface IProducer;

internal interface IProducer<in TMessage> : IProducer
    where TMessage : struct, IMessage
{
    void Produce(TMessage message);
}
