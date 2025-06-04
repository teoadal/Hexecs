using Hexecs.Assets.Development;

namespace Hexecs.Assets;

/// <summary>
/// Структура, представляющая типизированный ассет с компонентом типа <typeparamref name="T1"/>.
/// Обеспечивает доступ к компонентам ассета и предоставляет операции над ассетами.
/// </summary>
/// <typeparam name="T1">Тип компонента ассета, должен быть структурой и реализовывать интерфейс <see cref="IAssetComponent"/>.</typeparam>
[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(AssetDebugProxy<>))]
public readonly struct Asset<T1> : IEquatable<Asset<T1>>
    where T1 : struct, IAssetComponent
{
    /// <summary>
    /// Возвращает пустой экземпляр ассет с компонентом.
    /// </summary>
    public static Asset<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, Asset.EmptyId);
    }

    /// <summary>
    /// Первый компонент ассета.
    /// </summary>
    public T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context.GetComponent<T1>(Id);
    }

    /// <summary>
    /// Определяет, является ли ассет пустым.
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
    /// Уникальный идентификатор ассета.
    /// </summary>
    public readonly uint Id;

    /// <summary>
    /// Инициализирует новый экземпляр ассета.
    /// </summary>
    /// <param name="context">Контекст ассета.</param>
    /// <param name="id">Идентификатор ассета.</param>
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
    /// Определяет, равен ли текущий ассет указанному ассету.
    /// </summary>
    /// <param name="other">Ассет для сравнения с текущим.</param>
    /// <returns>Возвращает true, если ассеты равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Asset<T1> other) => Id == other.Id && ReferenceEquals(Context, other.Context);

    /// <summary>
    /// Определяет, равен ли текущий ассет указанному объекту.
    /// </summary>
    /// <param name="obj">Объект для сравнения с текущим ассетом.</param>
    /// <returns>Возвращает true, если объект является ассетом того же типа и равен текущему; иначе false.</returns>
    public override bool Equals(object? obj) => obj switch
    {
        Asset<T1> other => Equals(other),
        Asset asset => asset.Is<T1>(out var expected) && Equals(expected),
        _ => false
    };

    /// <summary>
    /// Возвращает хеш-код для текущего ассета.
    /// </summary>
    /// <returns>Хеш-код для текущего ассета.</returns>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// Оператор равенства для ассетов.
    /// </summary>
    /// <param name="left">Первый ассет для сравнения.</param>
    /// <param name="right">Второй ассет для сравнения.</param>
    /// <returns>Возвращает true, если ассеты равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Asset<T1> left, in Asset<T1> right) => left.Equals(right);

    /// <summary>
    /// Оператор неравенства для ассетов.
    /// </summary>
    /// <param name="left">Первый ассет для сравнения.</param>
    /// <param name="right">Второй ассет для сравнения.</param>
    /// <returns>Возвращает true, если ассеты не равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Asset<T1> left, in Asset<T1> right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование ассета в булево значение.
    /// Возвращает true, если ассет не пустой.
    /// </summary>
    /// <param name="asset">Ассет для преобразования.</param>
    /// <returns>Возвращает true, если ассет не пустой; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in Asset<T1> asset) => !asset.IsEmpty;

    /// <summary>
    /// Неявное преобразование ассета в идентификатор ассета.
    /// </summary>
    /// <param name="asset">Ассет для преобразования.</param>
    /// <returns>Идентификатор ассета.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId(in Asset<T1> asset) => new(asset.Id);

    /// <summary>
    /// Неявное преобразование ассета в типизированный идентификатор ассета.
    /// </summary>
    /// <param name="asset">Ассет для преобразования.</param>
    /// <returns>Типизированный идентификатор ассета.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetId<T1>(in Asset<T1> asset) => new(asset.Id);

    /// <summary>
    /// Неявное преобразование типизированного ассета в нетипизированный ассет.
    /// </summary>
    /// <param name="asset">Типизированный ассет для преобразования.</param>
    /// <returns>Нетипизированный ассет.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Asset(in Asset<T1> asset) => new(asset.Context, asset.Id);

    #endregion
}