namespace Hexecsm.Components;

/// <summary>
/// Структура, представляющая собой идентификатор типа компонента в системе.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public readonly struct ComponentTypeId : IEquatable<ComponentTypeId>
{
    /// <summary>
    /// Константа, представляющая собой пустой номер типа компонента.
    /// </summary>
    private const ushort EmptyId = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentTypeId Unsafe(ushort componentTypeId)
    {
        return new ComponentTypeId(componentTypeId);
    }

    /// <summary>
    /// Пустой идентификатор актёра, используемый по умолчанию.
    /// </summary>
    public static ComponentTypeId Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new ComponentTypeId(EmptyId);
    }

    /// <summary>
    /// Значение идентификатора актёра.
    /// </summary>
    public readonly ushort Value;

    /// <summary>
    /// Проверяет, является ли идентификатор пустым.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value == EmptyId;
    }

    /// <summary>
    /// Проверяет, является ли идентификатор НЕ пустым.
    /// </summary>
    public bool IsNotEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value != EmptyId;
    }

    /// <summary>
    /// Внутренний конструктор для создания идентификатора актёра.
    /// </summary>
    /// <param name="value">Значение идентификатора.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ComponentTypeId(ushort value)
    {
        Value = value;
    }

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ComponentTypeId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ComponentTypeId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ComponentTypeId left, in ComponentTypeId right)
    {
        return left.Value == right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ComponentTypeId left, in ComponentTypeId right)
    {
        return left.Value != right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(in ComponentTypeId left, in ComponentTypeId right)
    {
        return left.Value < right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(in ComponentTypeId left, in ComponentTypeId right)
    {
        return left.Value > right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(in ComponentTypeId left, in ComponentTypeId right)
    {
        return left.Value <= right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(in ComponentTypeId left, in ComponentTypeId right)
    {
        return left.Value >= right.Value;
    }

    #endregion

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ComponentTypeId actor)
    {
        return actor.IsNotEmpty;
    }

    #endregion
}
