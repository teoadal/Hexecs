namespace Hexecs.Values;

/// <summary>
/// Таблица ключ-значение, которая хранит данные по принципу словаря.
/// </summary>
public interface IValueTable
{
    /// <summary>
    /// Имя таблицы.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Тип ключа, используемого в таблице.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Тип значения, хранимого в таблице.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Очищает все записи в таблице.
    /// </summary>
    void Clear();
}

/// <summary>
/// Таблица ключ-значение, которая хранит данные по принципу словаря,
/// где тип ключа известен во время компиляции.
/// </summary>
/// <typeparam name="TKey">Тип ключа, который должен быть не null.</typeparam>
public interface IValueTable<in TKey> : IValueTable
    where TKey : notnull
{
    Type IValueTable.KeyType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => typeof(TKey);
    }

    /// <summary>
    /// Проверяет, существует ли запись с указанным ключом в таблице.
    /// </summary>
    /// <param name="key">Ключ для проверки.</param>
    /// <returns>True, если запись существует; иначе false.</returns>
    bool Has(TKey key);

    /// <summary>
    /// Удаляет запись с указанным ключом из таблицы.
    /// </summary>
    /// <param name="key">Ключ записи, которую нужно удалить.</param>
    /// <returns>True, если запись была удалена; false, если записи с указанным ключом не существует.</returns>
    bool Remove(TKey key);
}

/// <summary>
/// Таблица ключ-значение, которая хранит данные по принципу словаря,
/// где типы ключа и значения известны во время компиляции.
/// </summary>
/// <typeparam name="TKey">Тип ключа, который должен быть не null.</typeparam>
/// <typeparam name="TValue">Тип значения, который должен быть структурой.</typeparam>
public interface IValueTable<TKey, TValue> : IValueTable<TKey>, IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
    where TValue : notnull
{
    Type IValueTable.ValueType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => typeof(TValue);
    }

    /// <summary>
    /// Получает значение, связанное с указанным ключом.
    /// </summary>
    /// <param name="key">Ключ, для которого нужно получить значение.</param>
    /// <returns>Значение, связанное с ключом.</returns>
    /// <exception cref="KeyNotFoundException">Выбрасывается, если ключ не найден в таблице.</exception>
    TValue Get(TKey key);
    
    /// <summary>
    /// Проверяет, содержит ли таблица указанную пару ключ-значение.
    /// </summary>
    /// <param name="key">Ключ для проверки.</param>
    /// <param name="value">Значение для проверки.</param>
    /// <returns>True, если таблица содержит запись с указанным ключом и значением; иначе false.</returns>
    bool Has(TKey key, TValue value);

    /// <summary>
    /// Удаляет запись с указанным ключом и возвращает ее значение.
    /// </summary>
    /// <param name="key">Ключ записи, которую нужно удалить.</param>
    /// <param name="value">Выходной параметр, который получает значение удаленной записи, если операция успешна.</param>
    /// <returns>True, если запись была удалена; false, если запись с указанным ключом не существует.</returns>
    bool Remove(TKey key, out TValue value);

    /// <summary>
    /// Устанавливает значение для указанного ключа.
    /// Если ключ уже существует, его значение обновляется.
    /// </summary>
    /// <param name="key">Ключ, для которого нужно установить значение.</param>
    /// <param name="value">Значение, которое нужно установить.</param>
    void Set(TKey key, TValue value);

    /// <summary>
    /// Пытается получить значение, связанное с указанным ключом.
    /// </summary>
    /// <param name="key">Ключ, для которого нужно получить значение.</param>
    /// <param name="value">Выходной параметр, который получает значение, связанное с ключом, если операция успешна.</param>
    /// <returns>True, если ключ найден; иначе false.</returns>
    bool TryGet(TKey key, out TValue value);
}