using Hexecs.Arch.Components;

namespace Hexecs.Arch.Archetypes;

internal sealed class ArchetypeFactory
{
    private ushort _nextArchetypeId;

    public ArchetypeDefault CreateDefault() => new(_nextArchetypeId++);

    public IArchetype Create(ArchetypeSign sign)
    {
        var typeParams = sign.ComponentIds
            .Select(ActorComponentType.GetType)
            .ToArray();

        var type = typeParams.Length switch
        {
            0 => throw new NotSupportedException("Use default archetype for empty sign"),
            1 => typeof(Archetype<>).MakeGenericType(typeParams),
            2 => typeof(Archetype<,>).MakeGenericType(typeParams),
            3 => typeof(Archetype<,,>).MakeGenericType(typeParams),
            _ => throw new NotImplementedException("Archetype not supported")
        };

        return (IArchetype)Activator.CreateInstance(
            type: type,
            args: [_nextArchetypeId++])!;
    }
}