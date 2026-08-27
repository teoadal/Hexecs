using Hexecs.Actors.Components;

namespace Hexecs.Actors.Nodes;

[StructLayout(LayoutKind.Sequential, Size = 32)]
internal struct ActorNodeComponent : IActorComponent
{
    public ActorId NextSiblingId;
    public ActorId FirstChildId;
    public ActorId ParentId;
    public ActorId PrevSiblingId;
    public uint ChildCount;

    public static ActorComponentConfiguration<ActorNodeComponent> CreatePoolConfiguration()
    {
        return new ActorComponentConfiguration<ActorNodeComponent>(
            null,
            null,
            null,
            ActorNodeComponentConverter.Instance);
    }
}