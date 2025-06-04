using Hexecs.Actors;

namespace Hexecs.Utils;

/// <summary>
/// Класс Args представляет собой пул для хранения и передачи аргументов в типизированном виде.
/// </summary>
public sealed class Args : IEnumerable<KeyValuePair<string, object>>
{
    [ThreadStatic] private static Args? _instance;

    /// <summary>
    /// Получает экземпляр Args из пула или создает новый, если пул пуст.
    /// </summary>
    public static Args Rent() => Interlocked.Exchange(ref _instance, null) ?? new Args();

    /// <summary>
    /// Получает экземпляр Args из пула и устанавливает одно значение.
    /// </summary>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="name">Имя аргумента.</param>
    /// <param name="value">Значение аргумента.</param>
    public static Args Rent<TValue>(string name, TValue value) => Rent().Set(name, value);

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
        if (!TryGet<TValue>(name, out var value))
        {
            ActorError.ValueNotFound(name, typeof(TValue));
        }

        return value;
    }

    /// <summary>
    /// Возвращает экземпляр Args в пул после использования.
    /// Очищает все хранилища значений и возвращает их в соответствующие пулы.
    /// </summary>
    public void Return()
    {
        foreach (var storage in _values.Values)
        {
            storage.Return();
        }

        _values.Clear();
        Interlocked.Exchange(ref _instance, this);
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
        if (!_values.TryGetValue(typeof(TValue), out var storage))
        {
            value = default!;
            return false;
        }

        var expectedValues = Unsafe.As<ValueStorage<TValue>>(storage);
        if (expectedValues.TryGetValue(name, out var existsValue))
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
        var key = typeof(TValue);

        if (!_values.TryGetValue(key, out var storage))
        {
            storage = ValueStorage<TValue>.RentStorage();
            _values.Add(key, storage);
        }

        var expectedValues = Unsafe.As<ValueStorage<TValue>>(storage);
        expectedValues[name] = value;

        return this;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.Values
        .SelectMany(static storage => storage.Enumerate())
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
        [ThreadStatic] private static ValueStorage<TValue>? _storageInstance;

        public static ValueStorage<TValue> RentStorage()
        {
            return Interlocked.Exchange(ref _storageInstance, null) ?? new ValueStorage<TValue>();
        }

        public IEnumerable<KeyValuePair<string, object>> Enumerate()
        {
            return this.Select(static value => new KeyValuePair<string, object>(value.Key, value.Value!));
        }

        public void Return()
        {
            Clear();
            Interlocked.Exchange(ref _storageInstance, this);
        }
    }
}