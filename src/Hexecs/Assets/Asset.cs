using Hexecs.Assets.Development;

namespace Hexecs.Assets;

/// <summary>
/// Дескриптор ассета, представляющий собой ссылку на компоненты ассета в его контексте.
/// Является легковесным типом-значением для эффективной передачи ассетов по ссылке.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(AssetDebugProxy))]
public readonly struct Asset : IEquatable<Asset>
{
    /// <summary>
    /// Константа, представляющая идентификатор пустого ассета.
    /// </summary>
    internal const uint EmptyId = 0;

    /// <summary>
    /// Возвращает пустой экземпляр ассета.
    /// </summary>
    public static Asset Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, EmptyId);
    }

    /// <summary>
    /// Проверяет, является ли ассет пустым (отсутствует контекст).
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context == null;
    }

    /// <summary>
    /// Контекст ассета, управляющий его жизненным циклом и содержащий коллекции компонентов.
    /// </summary>
    public readonly AssetContext Context;

    /// <summary>
    /// Уникальный идентификатор ассета в контексте.
    /// </summary>
    public readonly uint Id;

    /// <summary>
    /// Создает новый экземпляр ассета с указанным контекстом и идентификатором.
    /// </summary>
    /// <param name="context">Контекст ассета</param>
    /// <param name="id">Идентификатор ассета</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Asset(AssetContext context, uint id)
    {
        Context = context;
        Id = id;
    }

    /// <summary>
    /// Преобразует ассет в типизированный ассет с указанным компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента ассета</typeparam>
    /// <exception cref="Exception">Выбрасывает ошибку, если компонент отсутствует в ассете</exception>
    /// <returns>Типизированный ассет</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset<T> As<T>() where T : struct, IAssetComponent => Context.GetAsset<T>(Id);

    /// <summary>
    /// Преобразует ассет в ссылку на типизированный ассет с указанным компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента ассета</typeparam>
    /// <exception cref="Exception">Выбрасывает ошибку, если компонент отсутствует в ассете</exception>
    /// <returns>Ссылка на типизированный ассет</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AssetRef<T> AsRef<T>() where T : struct, IAssetComponent => Context.GetAssetRef<T>(Id);

    /// <summary>
    /// Получает компонент указанного типа для данного ассета.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <exception cref="Exception">Выбрасывает ошибку, если компонент отсутствует в ассете</exception>
    /// <returns>Ссылка на компонент</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T Get<T>() where T : struct, IAssetComponent => ref Context.GetComponent<T>(Id);

    /// <summary>
    /// Проверяет наличие компонента указанного типа у ассета.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <returns>Возвращает true, если компонент существует; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, IAssetComponent => Context.HasComponent<T>(Id);

    /// <summary>
    /// Проверяет, является ли ассет ассетом с указанным типом компонента.
    /// В случае успеха возвращает типизированный ассет.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="asset">Результирующий типизированный ассет</param>
    /// <returns>Возвращает true, если ассет содержит указанный компонент; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is<T>(out Asset<T> asset) where T : struct, IAssetComponent => Context.TryGetAsset(Id, out asset);

    /// <summary>
    /// Проверяет, является ли ассет ссылкой на ассет с указанным типом компонента.
    /// В случае успеха возвращает ссылку на типизированный ассет.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="asset">Результирующая ссылка на типизированный ассет</param>
    /// <returns>Возвращает true, если ассет содержит указанный компонент; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRef<T>(out AssetRef<T> asset) where T : struct, IAssetComponent
    {
        return Context.TryGetAssetRef(Id, out asset);
    }

    /// <summary>
    /// Возвращает строковое представление ассета.
    /// Для пустого ассета возвращает специальное значение, иначе - описание из контекста.
    /// </summary>
    public override string ToString() => Context == null 
        ? StringUtils.EmptyValue 
        : Context.GetDescription(Id);
    
    #region Equality

    /// <summary>
    /// Проверяет равенство с другим ассетом. Ассеты равны, если у них одинаковые Id и они
    /// ссылаются на один и тот же контекст.
    /// </summary>
    /// <param name="other">Ассет для сравнения</param>
    /// <returns>Возвращает true, если ассеты равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Asset other) => Id == other.Id && ReferenceEquals(Context, other.Context);

    /// <summary>
    /// Проверяет равенство с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения</param>
    /// <returns>Возвращает true, если объект является ассетом и равен текущему; иначе false</returns>
    public override bool Equals(object? obj) => obj is Asset other && Equals(other);

    /// <summary>
    /// Вычисляет хеш-код ассета на основе его идентификатора.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// Оператор равенства для ассетов.
    /// </summary>
    /// <param name="left">Первый ассет</param>
    /// <param name="right">Второй ассет</param>
    /// <returns>Возвращает true, если ассеты равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Asset left, in Asset right) => left.Equals(right);

    /// <summary>
    /// Оператор неравенства для ассетов.
    /// </summary>
    /// <param name="left">Первый ассет</param>
    /// <param name="right">Второй ассет</param>
    /// <returns>Возвращает true, если ассеты не равны; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Asset left, in Asset right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование ассета в булево значение.
    /// Возвращает true, если ассет не пустой.
    /// </summary>
    /// <param name="asset">Ассет для преобразования</param>
    /// <returns>Возвращает true, если ассет не пустой; иначе false</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in Asset asset) => !asset.IsEmpty;

    /// <summary>
    /// Неявное преобразование ассета в идентификатор ассета.
    /// </summary>
    /// <param name="asset">Ассет для преобразования</param>
    /// <returns>Идентификатор ассета</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId(in Asset asset) => new(asset.Id);
    
    #endregion
}