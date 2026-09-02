namespace Hexecsm.Events;

public delegate void ConsumerDelegate<T>(in T message)
    where T : struct, IMessage;
