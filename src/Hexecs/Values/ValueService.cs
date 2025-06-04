using System.Collections.Frozen;

namespace Hexecs.Values;

/// <summary>
/// Предоставляет сервис для управления и доступа к различным таблицам ключ-значение.
/// Поддерживает операции чтения, записи и удаления данных с типизированным 
/// доступом к таблицам.
/// </summary>
public sealed class ValueService
{
    public static ValueService Empty => new([]);

    private readonly FrozenDictionary<string, IValueTable> _tables;

    internal ValueService(List<IValueTable> tables)
    {
        _tables = tables.ToFrozenDictionary(table => table.Name);
    }

    /// <summary>
    /// Очищает все таблицы, удаляя все данные из них.
    /// </summary>
    public void Clear()
    {
        foreach (var table in _tables.Values)
        {
            table.Clear();
        }
    }

    /// <summary>
    /// Очищает указанную таблицу, удаляя все данные из неё.
    /// </summary>
    /// <param name="tableName">Имя таблицы для очистки.</param>
    /// <returns><c>true</c>, если таблица найдена и очищена; иначе <c>false</c>.</returns>
    public bool ClearTable(string tableName)
    {
        if (!_tables.TryGetValue(tableName, out var table)) return false;

        table.Clear();
        return true;
    }

    /// <summary>
    /// Получает таблицу ключ-значение по имени.
    /// </summary>
    /// <param name="tableName">Имя запрашиваемой таблицы.</param>
    /// <returns>Таблица ключ-значение, если она существует; иначе генерирует ошибку.</returns>
    /// <exception cref="Exception">Возникает, если таблица не найдена.</exception>
    public IValueTable GetTable(string tableName)
    {
        if (_tables.TryGetValue(tableName, out var table)) return table;

        ValueError.TableNotFound(tableName);
        return null;
    }

    /// <summary>
    /// Получает типизированную таблицу ключ-значение с указанным типом ключа.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа в таблице.</typeparam>
    /// <param name="tableName">Имя запрашиваемой таблицы.</param>
    /// <returns>Типизированная таблица ключ-значение, если она существует и соответствует указанному типу ключа; иначе генерирует ошибку.</returns>
    /// <exception cref="Exception">Возникает, если таблица не найдена или имеет неправильный тип ключа.</exception>
    public IValueTable<TKey> GetTable<TKey>(string tableName)
        where TKey : notnull
    {
        if (_tables.TryGetValue(tableName, out var table))
        {
            if (table is IValueTable<TKey> expected)
            {
                return expected;
            }

            ValueError.TableNotExpected(tableName, table.KeyType);
            return null;
        }

        ValueError.TableNotFound(tableName);
        return null;
    }

    /// <summary>
    /// Получает типизированную таблицу ключ-значение с указанными типами ключа и значения.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа в таблице.</typeparam>
    /// <typeparam name="TValue">Тип значения в таблице.</typeparam>
    /// <param name="tableName">Имя запрашиваемой таблицы.</param>
    /// <returns>Типизированная таблица ключ-значение, если она существует и соответствует указанным типам; иначе генерирует ошибку.</returns>
    /// <exception cref="Exception">Возникает, если таблица не найдена или имеет неправильные типы.</exception>
    public IValueTable<TKey, TValue> GetTable<TKey, TValue>(string tableName)
        where TKey : notnull
        where TValue : notnull
    {
        if (_tables.TryGetValue(tableName, out var table))
        {
            if (table is IValueTable<TKey, TValue> expected)
            {
                return expected;
            }

            ValueError.TableNotExpected(tableName, table.KeyType, table.ValueType);
            return null;
        }

        ValueError.TableNotFound(tableName);
        return null;
    }

    /// <summary>
    /// Получает значение по ключу из указанной таблицы.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="key">Ключ для поиска.</param>
    /// <returns>Значение, соответствующее указанному ключу.</returns>
    public TValue GetValue<TKey, TValue>(string tableName, TKey key)
        where TKey : notnull
        where TValue : notnull
    {
        var table = GetTable<TKey, TValue>(tableName);
        return table.Get(key);
    }

    /// <summary>
    /// Получает все пары ключ-значение из указанной таблицы.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <returns>Перечисление всех пар ключ-значение в таблице.</returns>
    public IEnumerable<KeyValuePair<TKey, TValue>> GetValues<TKey, TValue>(string tableName)
        where TKey : notnull
        where TValue : notnull
    {
        var table = GetTable<TKey, TValue>(tableName);
        return table;
    }

    /// <summary>
    /// Проверяет, существует ли таблица с указанным именем.
    /// </summary>
    /// <param name="tableName">Имя проверяемой таблицы.</param>
    /// <returns><c>true</c>, если таблица существует; иначе <c>false</c>.</returns>
    public bool HasTable(string tableName) => _tables.ContainsKey(tableName);

    /// <summary>
    /// Проверяет, существует ли значение по указанному ключу в таблице.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="key">Ключ для проверки.</param>
    /// <returns><c>true</c>, если значение существует; иначе <c>false</c>.</returns>
    public bool HasValue<TKey>(string tableName, TKey key)
        where TKey : notnull
    {
        var table = GetTable<TKey>(tableName);
        return table.Has(key);
    }

    /// <summary>
    /// Проверяет, соответствует ли указанное значение ключу в таблице.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="key">Ключ для проверки.</param>
    /// <param name="value">Значение для сравнения.</param>
    /// <returns><c>true</c>, если ключ существует и значение соответствует; иначе <c>false</c>.</returns>
    public bool HasValue<TKey, TValue>(string tableName, TKey key, TValue value)
        where TKey : notnull
        where TValue : notnull
    {
        var table = GetTable<TKey, TValue>(tableName);
        return table.Has(key, value);
    }

    /// <summary>
    /// Удаляет значение по ключу из указанной таблицы.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="key">Ключ для удаления.</param>
    /// <returns><c>true</c>, если значение было удалено; <c>false</c>, если таблица или ключ не найдены.</returns>
    public bool RemoveValue<TKey>(string tableName, TKey key)
        where TKey : notnull
    {
        if (_tables.TryGetValue(tableName, out var table) &&
            table is IValueTable<TKey> expectedTable)
        {
            return expectedTable.Remove(key);
        }

        return false;
    }

    /// <summary>
    /// Удаляет значение по ключу из указанной таблицы и возвращает удаленное значение.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="key">Ключ для удаления.</param>
    /// <param name="value">Удаленное значение, если операция успешна; иначе значение по умолчанию.</param>
    /// <returns><c>true</c>, если значение было удалено; <c>false</c>, если таблица или ключ не найдены.</returns>
    public bool RemoveValue<TKey, TValue>(string tableName, TKey key, out TValue? value)
        where TKey : notnull
        where TValue : notnull
    {
        if (_tables.TryGetValue(tableName, out var table) &&
            table is IValueTable<TKey, TValue> expectedTable)
        {
            return expectedTable.Remove(key, out value);
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Устанавливает значение для указанного ключа в таблице.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="key">Ключ для установки значения.</param>
    /// <param name="value">Значение для сохранения.</param>
    public void SetValue<TKey, TValue>(string tableName, TKey key, TValue value)
        where TKey : notnull
        where TValue : notnull
    {
        var table = GetTable<TKey, TValue>(tableName);
        table.Set(key, value);
    }

    /// <summary>
    /// Пытается получить значение по ключу из указанной таблицы.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="tableName">Имя таблицы.</param>
    /// <param name="key">Ключ для поиска.</param>
    /// <param name="value">Найденное значение, если ключ существует; иначе значение по умолчанию.</param>
    /// <returns><c>true</c>, если значение найдено; иначе <c>false</c>.</returns>
    public bool TryGetValue<TKey, TValue>(string tableName, TKey key, out TValue? value)
        where TKey : notnull
        where TValue : notnull
    {
        if (_tables.TryGetValue(tableName, out var table) && table is IValueTable<TKey, TValue> expectedTable)
        {
            return expectedTable.TryGet(key, out value);
        }

        value = default!;
        return false;
    }
}