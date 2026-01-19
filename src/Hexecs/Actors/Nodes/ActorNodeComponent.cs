using Hexecs.Actors.Components;

namespace Hexecs.Actors.Nodes;

[StructLayout(LayoutKind.Sequential, Size = 32)]
internal struct ActorNodeComponent : IActorComponent
{
    public uint NextSiblingId;
    public uint FirstChildId;
    public uint ParentId;
    public uint PrevSiblingId;
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