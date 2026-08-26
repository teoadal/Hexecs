using Hexecs.Actors;

namespace Hexecs.Utils;

/// <summary>
/// Класс Args представляет собой пул для хранения и передачи аргументов в типизированном виде.
/// </summary>
public sealed class Args : IEnumerable<KeyValuePair<string, object>>
{
    [ThreadStatic]
    private static Args? Instance;

    /// <summary>
    /// Получает экземпляр Args из пула или создает новый, если пул пуст.
    /// </summary>
    public static Args Rent()
    {
        return Interlocked.Exchange(ref Instance, null) ?? new Args();
    }

    /// <summary>
    /// Получает экземпляр Args из пула и устанавливает одно значение.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="name">Имя аргумента.</param>
    /// <param name="value">Значение аргумента.</param>
    public static Args Rent<TValue>(string name, TValue value)
    {
        return Rent().Set(name, value);
    }

    private readonly Dictionary<Type, IValueStorage> _values;

    private Args()
    {
        _values = new Dictionary<Type, IValueStorage>(4, ReferenceComparer<Type>.Instance);
    }

    /// <summary>
    /// Получает значение аргумента по имени.
    /// Выбрасывает исключение, если значение не найдено.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="name">Имя аргумента.</param>
    public TValue Get<TValue>(string name)
    {
        if (!TryGet(name, out TValue value))
        {
            ActorError.ValueNotFound(name, typeof(TValue));
        }

        return value;
    }

    /// <summary>
    /// Получает значение аргумента по имени.
    /// Выбрасывает исключение, если значение не найдено.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="name">Имя аргумента.</param>
    public TValue GetOrDefault<TValue>(string name)
    {
        return TryGet(name, out TValue value) ? value : default!;
    }

    /// <summary>
    /// Получает значение аргумента по имени.
    /// Выбрасывает исключение, если значение не найдено.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="name">Имя аргумента.</param>
    /// <param name="defaultValue">Значение по умолчанию</param>
    public TValue GetOrDefault<TValue>(string name, TValue defaultValue)
    {
        return TryGet(name, out TValue value) ? value : defaultValue;
    }

    /// <summary>
    /// Возвращает экземпляр Args в пул после использования.
    /// Очищает все хранилища значений и возвращает их в соответствующие пулы.
    /// </summary>
    public void Return()
    {
        foreach (IValueStorage storage in _values.Values)
        {
            storage.Return();
        }

        _values.Clear();
        Interlocked.Exchange(ref Instance, this);
    }

    /// <summary>
    /// Пытается получить значение аргумента по имени.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="name">Имя аргумента.</param>
    /// <param name="value">Переменная для хранения найденного значения.</param>
    /// <returns>Возвращает true, если значение найдено; в противном случае — false.</returns>
    public bool TryGet<TValue>(string name, out TValue value)
    {
        if (!_values.TryGetValue(typeof(TValue), out IValueStorage? storage))
        {
            value = default!;

            return false;
        }

        var expectedValues = Unsafe.As<ValueStorage<TValue>>(storage);

        if (expectedValues.TryGetValue(name, out TValue? existsValue))
        {
            value = existsValue;

            return true;
        }

        value = default!;

        return false;
    }

    /// <summary>
    /// Устанавливает значение аргумента.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="name">Имя аргумента.</param>
    /// <param name="value">Значение аргумента.</param>
    public Args Set<TValue>(string name, TValue value)
    {
        Type key = typeof(TValue);

        if (!_values.TryGetValue(key, out IValueStorage? storage))
        {
            storage = ValueStorage<TValue>.RentStorage();
            _values.Add(key, storage);
        }

        var expectedValues = Unsafe.As<ValueStorage<TValue>>(storage);
        expectedValues[name] = value;

        return this;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _values.Values
            .SelectMany(static storage => storage.Enumerate())
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Интерфейс для хранилища значений определенного типа.
    /// </summary>
    private interface IValueStorage
    {
        IEnumerable<KeyValuePair<string, object>> Enumerate();

        void Return();
    }

    [DebuggerDisplay("Type {typeof(TValue).Name}, Count = {Count}")]
    private sealed class ValueStorage<TValue> : Dictionary<string, TValue>, IValueStorage
    {
        [ThreadStatic]
        private static ValueStorage<TValue>? StorageInstance;

        public static ValueStorage<TValue> RentStorage()
        {
            return Interlocked.Exchange(ref StorageInstance, null) ?? new ValueStorage<TValue>();
        }

        public IEnumerable<KeyValuePair<string, object>> Enumerate()
        {
            return this.Select(static value => new KeyValuePair<string, object>(value.Key, value.Value!));
        }

        public void Return()
        {
            Clear();
            Interlocked.Exchange(ref StorageInstance, this);
        }
    }
}
