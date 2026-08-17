namespace Hexecs.Arch.Archetypes;

public readonly struct ArchetypeSign
{
    public readonly ushort[] ComponentIds;

    public ArchetypeSign()
    {
        ComponentIds = [];
    }

    public ArchetypeSign(ushort component1)
    {
        ComponentIds = [component1];
    }

    public ArchetypeSign(ushort component1, ushort component2)
    {
        ComponentIds = [component1, component2];
        Array.Sort(ComponentIds);
    }

    public ArchetypeSign(ushort component1, ushort component2, ushort component3)
    {
        ComponentIds = [component1, component2, component3];
        Array.Sort(ComponentIds);
    }

    internal ArchetypeSign(ushort[] sign)
    {
        ComponentIds = [.. sign];
        Array.Sort(ComponentIds);
    }

    private ArchetypeSign(ushort[] componentIds, bool alreadySorted)
    {
        ComponentIds = componentIds;

        if (!alreadySorted)
        {
            Array.Sort(ComponentIds);
        }
    }

    public bool Contains(ushort componentType1)
    {
        return Array.BinarySearch(ComponentIds, componentType1) >= 0;
    }

    public bool Contains(ushort componentType1, ushort otherComponentType2)
    {
        if (componentType1 == otherComponentType2)
        {
            return Contains(componentType1);
        }

        return Array.BinarySearch(ComponentIds, componentType1) >= 0 &&
               Array.BinarySearch(ComponentIds, otherComponentType2) >= 0;
    }

    public bool IsAddTransitionFrom(in ArchetypeSign other, ushort componentType)
    {
        if (other.Contains(componentType))
        {
            return false;
        }

        ReadOnlySpan<ushort> target = ComponentIds;
        ReadOnlySpan<ushort> source = other.ComponentIds;

        if (target.Length != source.Length + 1)
            return false;

        int targetIndex = 0;
        int sourceIndex = 0;

        bool componentMatched = false;

        while (targetIndex < target.Length)
        {
            ushort targetValue = target[targetIndex];

            if (!componentMatched && targetValue == componentType)
            {
                componentMatched = true;
                targetIndex++;
                continue;
            }

            if (sourceIndex >= source.Length)
                return false;

            if (targetValue != source[sourceIndex])
                return false;

            targetIndex++;
            sourceIndex++;
        }

        return componentMatched && sourceIndex == source.Length;
    }

    public bool IsRemoveTransitionFrom(in ArchetypeSign other, ushort componentType)
    {
        if (!other.Contains(componentType))
        {
            return false;
        }

        ReadOnlySpan<ushort> target = ComponentIds;
        ReadOnlySpan<ushort> source = other.ComponentIds;

        if (target.Length + 1 != source.Length)
        {
            return false;
        }

        int targetIndex = 0;
        int sourceIndex = 0;

        bool componentSkipped = false;

        while (sourceIndex < source.Length)
        {
            ushort sourceValue = source[sourceIndex];

            if (!componentSkipped && sourceValue == componentType)
            {
                componentSkipped = true;
                sourceIndex++;
                continue;
            }

            if (targetIndex >= target.Length)
            {
                return false;
            }

            if (target[targetIndex] != sourceValue)
            {
                return false;
            }

            targetIndex++;
            sourceIndex++;
        }

        return componentSkipped && targetIndex == target.Length;
    }

    public ArchetypeSign With(ushort componentType)
    {
        var index = Array.BinarySearch(ComponentIds, componentType);
        if (index >= 0)
        {
            return this;
        }

        var insertIndex = ~index;
        var result = new ushort[ComponentIds.Length + 1];

        if (insertIndex > 0)
        {
            Array.Copy(ComponentIds, 0, result, 0, insertIndex);
        }

        result[insertIndex] = componentType;

        if (insertIndex < ComponentIds.Length)
        {
            Array.Copy(
                ComponentIds,
                insertIndex,
                result,
                insertIndex + 1,
                ComponentIds.Length - insertIndex);
        }

        return new ArchetypeSign(result, alreadySorted: true);
    }

    public ArchetypeSign Without(ushort componentType)
    {
        var index = Array.BinarySearch(ComponentIds, componentType);
        if (index < 0)
        {
            return this;
        }

        if (ComponentIds.Length == 1)
        {
            return new ArchetypeSign();
        }

        var result = new ushort[ComponentIds.Length - 1];

        if (index > 0)
        {
            Array.Copy(ComponentIds, 0, result, 0, index);
        }

        if (index < ComponentIds.Length - 1)
        {
            Array.Copy(
                ComponentIds,
                index + 1,
                result,
                index,
                ComponentIds.Length - index - 1);
        }

        return new ArchetypeSign(result, alreadySorted: true);
    }
}