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
    /// Константа, представляющая идентификатор пустого ассета.
    /// </summary>
    internal const uint EmptyId = 0;

    /// <summary>
    /// Возвращает пустой идентификатор ассета.
    /// </summary>
    public static AssetId Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(EmptyId);
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
    /// Создает новый идентификатор ассета с указанным числовым значением.
    /// </summary>
    /// <param name="value">Числовое значение идентификатора</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal AssetId(uint value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return AssetMarshal.TryGetDebugContext(out var context)
            ? context.GetDescription(this)
            : IsEmpty
                ? StringUtils.EmptyValue
                : Value.ToString();
    }

    /// <summary>
    /// Преобразует идентификатор в <see cref="Asset"/>, используя указанный контекст.
    /// </summary>
    /// <param name="context">Контекст ассетов</param>
    /// <returns>Ассет</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset Unwrap(AssetContext context)
    {
        return context.GetAsset(this);
    }

    #region Equality

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(AssetId other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is AssetId other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in AssetId left, in AssetId right) => left.Value == right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in AssetId left, in AssetId right) => left.Value != right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(in AssetId left, in AssetId right) => left.Value < right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(in AssetId left, in AssetId right) => left.Value > right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(in AssetId left, in AssetId right) => left.Value <= right.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(in AssetId left, in AssetId right) => left.Value >= right.Value;

    #endregion

    #region Implicit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in AssetId asset) => asset.IsNotEmpty;

    #endregion
}