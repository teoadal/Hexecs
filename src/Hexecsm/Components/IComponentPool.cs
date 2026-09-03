namespace Hexecsm.Components;

internal interface IComponentPool : IDisposable
{
    int Length { get; }

    void Clone(ActorId source, ActorId target);

    bool Contains(ActorId actorId);

    void ProcessPostponedOperations();

    void Remove(ActorId actorId);
}

