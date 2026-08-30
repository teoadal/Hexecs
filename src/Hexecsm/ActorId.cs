namespace Hexecsm;

/// <summary>
/// Структура, представляющая собой идентификатор актёра в системе.
/// </summary>
/// <remarks>
/// Это легковесная структура, которая хранит только идентификатор актёра
/// и используется для указания на актёра без хранения контекста.
/// </remarks>
[DebuggerDisplay("{Value}")]
public readonly struct ActorId : IEquatable<ActorId>
{
    /// <summary>
    /// Константа, представляющая собой пустой номер актёра.
    /// </summary>
    internal const uint EmptyId = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActorId Unsafe(uint actorId)
    {
        return new ActorId(actorId);
    }

    /// <summary>
    /// Пустой идентификатор актёра, используемый по умолчанию.
    /// </summary>
    public static ActorId Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new ActorId(EmptyId);
    }

    /// <summary>
    /// Значение идентификатора актёра.
    /// </summary>
    public readonly uint Value;

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
    private ActorId(uint value)
    {
        Value = value;
    }

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ActorId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ActorId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ActorId left, in ActorId right)
    {
        return left.Value == right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ActorId left, in ActorId right)
    {
        return left.Value != right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(in ActorId left, in ActorId right)
    {
        return left.Value < right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(in ActorId left, in ActorId right)
    {
        return left.Value > right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(in ActorId left, in ActorId right)
    {
        return left.Value <= right.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(in ActorId left, in ActorId right)
    {
        return left.Value >= right.Value;
    }

    #endregion

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorId actor)
    {
        return actor.IsNotEmpty;
    }

    #endregion
}
