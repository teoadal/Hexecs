using Hexecs.Actors.Development;

namespace Hexecs.Actors;

/// <summary>
/// Структура, представляющая собой типизированный идентификатор актёра в системе.
/// </summary>
/// <remarks>
/// ActorId&lt;T1&gt; - это легковесная структура, которая хранит идентификатор актёра
/// и информацию о требуемом компоненте, используется для типобезопасной ссылки на актёра.
/// </remarks>
/// <typeparam name="T1">Тип компонента, который должен иметь актёр.</typeparam>
[DebuggerTypeProxy(typeof(ActorIdDebugProxy<>))]
[DebuggerDisplay("{ToString()}")]
public readonly struct ActorId<T1> : IEquatable<ActorId<T1>>
    where T1 : struct, IActorComponent
{
    /// <summary>
    /// Пустой типизированный идентификатор актёра, используемый по умолчанию.
    /// </summary>
    public static ActorId<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Actor.EmptyId);
    }

    /// <summary>
    /// Значение идентификатора актёра.
    /// </summary>
    public readonly uint Value;

    /// <summary>
    /// Внутренний конструктор для создания типизированного идентификатора актёра.
    /// </summary>
    /// <param name="value">Значение идентификатора.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ActorId(uint value)
    {
        Value = value;
    }

    /// <summary>
    /// Проверяет, является ли идентификатор пустым.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value == Actor.EmptyId;
    }

    /// <summary>
    /// Возвращает строковое представление идентификатора актёра.
    /// </summary>
    /// <returns>Строковое представление идентификатора.</returns>
    public override string ToString() => ActorMarshal.TryGetDebugContext(out var context)
        ? context.GetDescription(Value)
        : IsEmpty
            ? StringUtils.EmptyValue
            : Value.ToString();

    /// <summary>
    /// Преобразует типизированный идентификатор в актёра с указанным контекстом и компонентом.
    /// </summary>
    /// <param name="context">Контекст актёра.</param>
    /// <returns>Типизированный актёр с данным идентификатором и компонентом.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor<T1> Unwrap(ActorContext context) => context.GetActor<T1>(Value);

    #region Equality

    /// <summary>
    /// Проверяет равенство между двумя типизированными идентификаторами актёров.
    /// </summary>
    /// <param name="other">Идентификатор для сравнения.</param>
    /// <returns>Возвращает true, если идентификаторы равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ActorId<T1> other) => Value == other.Value;

    /// <summary>
    /// Проверяет равенство с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>Возвращает true, если объекты равны; иначе false.</returns>
    public override bool Equals(object? obj) => obj is ActorId<T1> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ActorId<T1> left, in ActorId<T1> right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ActorId<T1> left, in ActorId<T1> right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование типизированного идентификатора актёра в bool.
    /// </summary>
    /// <param name="actor">Идентификатор для преобразования.</param>
    /// <returns>Возвращает true, если идентификатор не пустой; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorId<T1> actor) => !actor.IsEmpty;

    #endregion
}