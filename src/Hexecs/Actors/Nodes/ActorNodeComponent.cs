using Hexecs.Actors.Components;

namespace Hexecs.Actors.Nodes;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal readonly struct ActorNodeComponent(ActorNode node) : IActorComponent, IDisposable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActorNodeComponent Create(uint ownerId) => new(new ActorNode(ownerId));

    public static ActorComponentConfiguration<ActorNodeComponent> CreatePoolConfiguration()
    {
        return new ActorComponentConfiguration<ActorNodeComponent>(
            null,
            null,
            DisposeHandler,
            ActorNodeComponentConverter.Instance);
    }

    public static void DisposeHandler(ref ActorNodeComponent component)
    {
        component.Dispose();
        component = new ActorNodeComponent(null!); // remove reference to Node for GC
    }

    public readonly ActorNode Node = node;

    public void Dispose()
    {
        Node.Dispose();
    }
}