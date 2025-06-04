using Hexecs.Assets.Development;

namespace Hexecs.Assets;

/// <summary>
/// Структура, представляющая идентификатор ассета в контексте проекта.
/// </summary>
/// <remarks>
/// Используется для легковесной ссылки на ассеты без прямого хранения контекста.
/// </remarks>
[DebuggerTypeProxy(typeof(AssetIdDebugProxy))]
[DebuggerDisplay("{ToString()}")]
public readonly struct AssetId : IEquatable<AssetId>
{
    /// <summary>
    /// Возвращает пустой идентификатор ассета.
    /// </summary>
    public static AssetId Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Asset.EmptyId);
    }

    /// <summary>
    /// Числовой идентификатор ассета.
    /// </summary>
    public readonly uint Value;

    /// <summary>
    /// Проверяет, является ли идентификатор пустым.
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value == Asset.EmptyId;
    }

    /// <summary>
    /// Создает новый идентификатор ассета с указанным числовым значением.
    /// </summary>
    /// <param name="value">Числовое значение идентификатора</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetId(uint value)
    {
        Value = value;
    }

    /// <summary>
    /// Возвращает строковое представление идентификатора ассета.
    /// </summary>
    public override string ToString() => AssetMarshal.TryGetDebugContext(out var context)
        ? context.GetDescription(Value)
        : IsEmpty
            ? StringUtils.EmptyValue
            : Value.ToString();

    /// <summary>
    /// Преобразует идентификатор в <see cref="Asset"/>, используя указанный контекст.
    /// </summary>
    /// <param name="context">Контекст ассетов</param>
    /// <returns>Ассет</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset Unwrap(AssetContext context) => context.GetAsset(Value);

    /// <summary>
    /// Преобразует идентификатор в <see cref="Asset{T}"/> с указанным компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента ассета</typeparam>
    /// <param name="context">Контекст ассетов</param>
    /// <returns>Типизированный ассет</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset<T> Unwrap<T>(AssetContext context) where T : struct, IAssetComponent => context.GetAsset<T>(Value);

    #region Equality

    /// <summary>
    /// Проверяет равенство с другим идентификатором ассета.
    /// </summary>
    /// <param name="other">Идентификатор ассета для сравнения</param>
    /// <returns>Возвращает true, если идентификаторы равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(AssetId other) => Value == other.Value;

    /// <summary>
    /// Проверяет равенство с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения</param>
    /// <returns>Возвращает true, если объект является идентификатором ассета и равен текущему; иначе false</returns>
    public override bool Equals(object? obj) => obj is AssetId other && Equals(other);

    /// <summary>
    /// Вычисляет хеш-код идентификатора ассета.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Value);

    /// <summary>
    /// Оператор равенства для идентификаторов ассетов.
    /// </summary>
    /// <param name="left">Первый идентификатор</param>
    /// <param name="right">Второй идентификатор</param>
    /// <returns>Возвращает true, если идентификаторы равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in AssetId left, in AssetId right) => left.Equals(right);

    /// <summary>
    /// Оператор неравенства для идентификаторов ассетов.
    /// </summary>
    /// <param name="left">Первый идентификатор</param>
    /// <param name="right">Второй идентификатор</param>
    /// <returns>Возвращает true, если идентификаторы не равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in AssetId left, in AssetId right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование идентификатора ассета в булево значение.
    /// Возвращает true, если идентификатор не пустой.
    /// </summary>
    /// <param name="asset">Идентификатор ассета</param>
    /// <returns>Возвращает true, если идентификатор не пустой; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in AssetId asset) => !asset.IsEmpty;

    #endregion
}