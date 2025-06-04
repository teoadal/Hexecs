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
    /// Пустой идентификатор актёра, используемый по умолчанию.
    /// </summary>
    public static ActorId Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Actor.EmptyId);
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
        get => Value == Actor.EmptyId;
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
    public Actor Unwrap(ActorContext context) => context.GetActor(Value);

    /// <summary>
    /// Преобразует идентификатор в типизированного актёра с указанным контекстом и компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="context">Контекст актёра.</param>
    /// <returns>Типизированный актёр с данным идентификатором и компонентом.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor<T> Unwrap<T>(ActorContext context) where T : struct, IActorComponent => context.GetActor<T>(Value);

    /// <summary>
    /// Возвращает строковое представление идентификатора актёра.
    /// </summary>
    /// <returns>Строковое представление идентификатора.</returns>
    public override string ToString() => ActorMarshal.TryGetDebugContext(out var context)
        ? context.GetDescription(Value)
        : IsEmpty
            ? StringUtils.EmptyValue
            : Value.ToString();

    #region Equality

    /// <summary>
    /// Проверяет равенство между двумя идентификаторами актёров.
    /// </summary>
    /// <param name="other">Идентификатор для сравнения.</param>
    /// <returns>Возвращает true, если идентификаторы равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ActorId other) => Value == other.Value;

    /// <summary>
    /// Проверяет равенство с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>Возвращает true, если объекты равны; иначе false.</returns>
    public override bool Equals(object? obj) => obj is ActorId other && Equals(other);

    /// <summary>
    /// Возвращает хеш-код для идентификатора актёра.
    /// </summary>
    /// <returns>Хеш-код идентификатора.</returns>
    public override int GetHashCode() => HashCode.Combine(Value);

    /// <summary>
    /// Сравнивает два идентификатора актёров на равенство.
    /// </summary>
    /// <param name="left">Левый идентификатор.</param>
    /// <param name="right">Правый идентификатор.</param>
    /// <returns>Возвращает true, если идентификаторы равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ActorId left, in ActorId right) => left.Equals(right);

    /// <summary>
    /// Сравнивает два идентификатора актёров на неравенство.
    /// </summary>
    /// <param name="left">Левый идентификатор.</param>
    /// <param name="right">Правый идентификатор.</param>
    /// <returns>Возвращает true, если идентификаторы не равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ActorId left, in ActorId right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование идентификатора актёра в bool.
    /// </summary>
    /// <param name="actor">Идентификатор для преобразования.</param>
    /// <returns>Возвращает true, если идентификатор не пустой; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorId actor) => !actor.IsEmpty;

    #endregion
}