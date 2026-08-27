using Hexecs.Actors.Development;

namespace Hexecs.Actors;

/// <summary>
/// Структура, представляющая собой идентификатор актёра в системе.
/// </summary>
/// <remarks>
/// ActorId - это легковесная структура, которая хранит только идентификатор актёра
/// и используется для ссылки на актёра без хранения контекста.
/// </remarks>
[DebuggerTypeProxy(typeof(ActorIdDebugProxy))]
[DebuggerDisplay("{ToString()}")]
public readonly struct ActorId : IEquatable<ActorId>
{
    /// <summary>
    /// Константа, представляющая собой пустой идентификатор актёра.
    /// </summary>
    internal const uint EmptyId = 0;

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
    internal ActorId(uint value)
    {
        Value = value;
    }

    /// <summary>
    /// Преобразует идентификатор в актёра с указанным контекстом.
    /// </summary>
    /// <param name="context">Контекст актёра.</param>
    /// <returns>Актёр с данным идентификатором.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor Unwrap(ActorContext context)
    {
        return context.GetActor(this);
    }

    public override string ToString()
    {
        return ActorMarshal.TryGetDebugContext(out ActorContext? context)
            ? context.GetDescription(this)
            : IsEmpty
                ? StringUtils.EmptyValue
                : Value.ToString();
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
