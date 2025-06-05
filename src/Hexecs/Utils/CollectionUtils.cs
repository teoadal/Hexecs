namespace Hexecs.Utils;

/// <summary>
/// Предоставляет статические методы-расширения для работы с коллекциями.
/// </summary>
public static class CollectionUtils
{
    /// <summary>
    /// Константа, возможно, используемая для обозначения начала списка свободных элементов
    /// в пользовательских структурах данных (точное использование зависит от контекста).
    /// </summary>
    public const int StartOfFreeList = -3;

    /// <summary>
    /// Выполняет указанное действие для каждого элемента перечисляемой коллекции.
    /// </summary>
    /// <typeparam name="T">Тип элементов в коллекции.</typeparam>
    /// <param name="collection">Коллекция, элементы которой нужно обработать.</param>
    /// <param name="action">Действие <see cref="Action{T}"/>, которое нужно выполнить для каждого элемента.</param>
    public static void Do<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var element in collection)
        {
            action(element);
        }
    }

    /// <summary>
    /// Выполняет указанное действие для каждого элемента массива.
    /// Это перегрузка для массивов для потенциальной оптимизации.
    /// </summary>
    /// <typeparam name="T">Тип элементов в массиве.</typeparam>
    /// <param name="collection">Массив, элементы которого нужно обработать.</param>
    /// <param name="action">Действие <see cref="Action{T}"/>, которое нужно выполнить для каждого элемента.</param>
    public static void Do<T>(this T[] collection, Action<T> action)
    {
        foreach (var element in collection)
        {
            action(element);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TResult> Select<T, TArg1, TResult>(
        this IEnumerable<T> collection,
        Func<T, TArg1, TResult> selector,
        TArg1 arg1)
    {
        return new SelectWithArgEnumerator<T, TArg1, TResult>(collection, selector, arg1);
    }

    /// <summary>
    /// Преобразует список <see cref="List{T}"/> в массив <typeparamref name="TValue"/>[],
    /// применяя к каждому элементу функцию выбора с использованием предоставленного контекста.
    /// </summary>
    /// <typeparam name="T">Тип элементов в исходном списке.</typeparam>
    /// <typeparam name="TValue">Тип элементов в результирующем массиве.</typeparam>
    /// <typeparam name="TContext">Тип контекста, передаваемого в функцию выбора.</typeparam>
    /// <param name="collection">Исходный список <see cref="List{T}"/>.</param>
    /// <param name="selector">Функция <see cref="Func{T, TContext, TValue}"/> для преобразования каждого элемента.</param>
    /// <param name="context">Контекст, передаваемый в функцию выбора <paramref name="selector"/>.</param>
    /// <returns>Новый массив <typeparamref name="TValue"/>[], содержащий преобразованные элементы. Возвращает пустой массив, если исходный список пуст.</returns>
    public static TValue[] ToArray<T, TValue, TContext>(
        this List<T> collection,
        Func<T, TContext, TValue> selector,
        TContext context)
    {
        if (collection.Count == 0) return [];

        var result = ArrayUtils.Create<TValue>(collection.Count);
        for (var i = 0; i < collection.Count; i++)
        {
            result[i] = selector(collection[i], context);
        }

        return result;
    }

    /// <summary>
    /// Преобразует перечисляемую коллекцию <see cref="IEnumerable{T}"/> в массив <typeparamref name="T"/>[].
    /// Пытается оптимизировать создание массива, определяя количество элементов заранее, если это возможно.
    /// В противном случае использует стандартный метод <see cref="System.Linq.Enumerable.ToArray{TSource}"/>.
    /// </summary>
    /// <typeparam name="T">Тип элементов в коллекции.</typeparam>
    /// <param name="collection">Исходная перечисляемая коллекция <see cref="IEnumerable{T}"/>.</param>
    /// <returns>Новый массив <typeparamref name="T"/>[], содержащий элементы из коллекции. Возвращает пустой массив, если исходная коллекция пуста.</returns>
    public static T[] ToArray<T>(IEnumerable<T> collection)
    {
        if (!collection.TryGetNonEnumeratedCount(out var count))
        {
            return collection.ToArray();
        }

        if (count == 0) return [];

        var array = ArrayUtils.Create<T>(count);
        var index = 0;
        foreach (var element in collection)
        {
            array[index++] = element;
        }

        return array;
    }

    /// <summary>
    /// Преобразует перечисляемую коллекцию <see cref="IEnumerable{T}"/> в массив <typeparamref name="T"/>[] указанной длины.
    /// Копирует элементы из коллекции в новый массив до тех пор, пока не будет достигнута указанная длина <paramref name="length"/>
    /// или пока элементы в коллекции не закончатся.
    /// </summary>
    /// <typeparam name="T">Тип элементов в коллекции.</typeparam>
    /// <param name="collection">Исходная перечисляемая коллекция <see cref="IEnumerable{T}"/>.</param>
    /// <param name="length">Требуемая длина результирующего массива.</param>
    /// <returns>
    /// Новый массив <typeparamref name="T"/>[] указанной длины. Если <paramref name="length"/> равен 0, возвращает пустой массив.
    /// Если коллекция содержит меньше элементов, чем <paramref name="length"/>, оставшиеся элементы массива будут иметь значение по умолчанию для типа <typeparamref name="T"/>.
    /// </returns>
    public static T[] ToArray<T>(IEnumerable<T> collection, int length)
    {
        if (length == 0) return [];

        var array = ArrayUtils.Create<T>(length);
        var index = 0;
        foreach (var element in collection)
        {
            if (index == length) break;
            array[index++] = element;
        }

        return array;
    }

    private sealed class SelectWithArgEnumerator<T, TArg1, TResult> : IEnumerable<TResult>, IEnumerator<TResult>
    {
        public TResult Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _selector(_collection.Current, _arg1);
        }

        private TArg1 _arg1;
        private IEnumerator<T> _collection;
        private Func<T, TArg1, TResult> _selector;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SelectWithArgEnumerator(IEnumerable<T> collection, Func<T, TArg1, TResult> selector, TArg1 arg1)
        {
            _arg1 = arg1;
            _collection = collection.GetEnumerator();
            _selector = selector;
        }

        public void Dispose()
        {
            _arg1 = default!;
            _collection.Dispose();
            _collection = null!;
            _selector = null!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<TResult> GetEnumerator() => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => _collection.MoveNext();

        public void Reset() => _collection.Reset();

        IEnumerator IEnumerable.GetEnumerator() => this;

        object? IEnumerator.Current => Current;
    }
}