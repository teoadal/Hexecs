using Hexecs.Assets.Development;

namespace Hexecs.Assets;

/// <summary>
/// Типизированный идентификатор ассета, гарантирующий наличие указанного компонента.
/// </summary>
/// <typeparam name="T1">Тип компонента ассета</typeparam>
[DebuggerTypeProxy(typeof(AssetIdDebugProxy<>))]
[DebuggerDisplay("{ToString()}")]
public readonly struct AssetId<T1> : IEquatable<AssetId<T1>>
    where T1 : struct, IAssetComponent
{
    /// <summary>
    /// Возвращает пустой типизированный идентификатор ассета.
    /// </summary>
    public static AssetId<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Asset.EmptyId);
    }

    /// <summary>
    /// Числовой идентификатор ассета.
    /// </summary>
    public readonly uint Value;

    /// <summary>
    /// Создает новый типизированный идентификатор ассета с указанным числовым значением.
    /// </summary>
    /// <param name="value">Числовое значение идентификатора</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetId(uint value)
    {
        Value = value;
    }

    /// <summary>
    /// Проверяет, является ли идентификатор пустым.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value == Asset.EmptyId;
    }

    /// <summary>
    /// Возвращает строковое представление типизированного идентификатора ассета.
    /// </summary>
    public override string ToString() => AssetMarshal.TryGetDebugContext(out var context)
        ? context.GetDescription(Value)
        : IsEmpty
            ? StringUtils.EmptyValue
            : Value.ToString();

    /// <summary>
    /// Преобразует типизированный идентификатор в <see cref="Asset{T}"/>, используя указанный контекст.
    /// </summary>
    /// <param name="context">Контекст ассетов</param>
    /// <returns>Типизированный ассет</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset<T1> Unwrap(AssetContext context) => context.GetAsset<T1>(Value);

    #region Equality

    /// <summary>
    /// Проверяет равенство с другим типизированным идентификатором ассета.
    /// </summary>
    /// <param name="other">Типизированный идентификатор ассета для сравнения</param>
    /// <returns>Возвращает true, если идентификаторы равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(AssetId<T1> other) => Value == other.Value;

    /// <summary>
    /// Проверяет равенство с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения</param>
    /// <returns>Возвращает true, если объект является типизированным идентификатором ассета и равен текущему; иначе false</returns>
    public override bool Equals(object? obj) => obj is AssetId<T1> other && Equals(other);

    /// <summary>
    /// Вычисляет хеш-код типизированного идентификатора ассета.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Value);

    /// <summary>
    /// Оператор равенства для типизированных идентификаторов ассетов.
    /// </summary>
    /// <param name="left">Первый идентификатор</param>
    /// <param name="right">Второй идентификатор</param>
    /// <returns>Возвращает true, если идентификаторы равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in AssetId<T1> left, in AssetId<T1> right) => left.Equals(right);

    /// <summary>
    /// Оператор неравенства для типизированных идентификаторов ассетов.
    /// </summary>
    /// <param name="left">Первый идентификатор</param>
    /// <param name="right">Второй идентификатор</param>
    /// <returns>Возвращает true, если идентификаторы не равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in AssetId<T1> left, in AssetId<T1> right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование типизированного идентификатора ассета в булево значение.
    /// Возвращает true, если идентификатор не пустой.
    /// </summary>
    /// <param name="asset">Типизированный идентификатор ассета</param>
    /// <returns>Возвращает true, если идентификатор не пустой; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in AssetId<T1> asset) => !asset.IsEmpty;

    #endregion
}