namespace Hexecs.Utils;

/// <summary>
/// Предоставляет набор высокопроизводительных утилит для работы с массивами.
/// Класс содержит методы для эффективного создания, изменения размера, вставки и удаления элементов массивов,
/// а также оптимизированные варианты этих операций с использованием пулов массивов.
/// </summary>
public static class ArrayUtils
{
    /// <summary>
    /// Создает новый неинициализированный массив заданной длины с оптимизированным выделением памяти.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="length">Требуемая длина массива</param>
    /// <returns>Новый массив типа T[] указанной длины</returns>
    /// <remarks>
    /// Метод использует <see cref="GC.AllocateUninitializedArray{T}"/> для более эффективного
    /// выделения памяти по сравнению со стандартным созданием массива.
    /// Элементы массива не инициализируются значениями по умолчанию.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Возникает, если параметр length меньше 0
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Create<T>(int length)
    {
        if (length == 0) return [];

        // ReSharper disable once InvertIf
        if (Environment.Is64BitProcess && typeof(T).IsPrimitive && length > 100)
        {
            // На 64-битной платформе для примитивных типов выравниваем размер по 16 байт
            var alignedLength = (length + 1) & ~1; // Выравнивание по границе 16 байт
            return GC.AllocateUninitializedArray<T>(alignedLength);
        }

        return GC.AllocateUninitializedArray<T>(length);
    }

    /// <summary>
    /// Очищает все элементы массива, устанавливая их значения по умолчанию.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для очистки</param>
    /// <remarks>
    /// Метод не выполняет никаких действий, если массив пуст.
    /// </remarks>
    public static void Clear<T>(T[] array)
    {
        if (array.Length == 0) return;
        Array.Clear(array);
    }

    /// <summary>
    /// Очищает указанное количество элементов массива, начиная с начала.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для очистки</param>
    /// <param name="length">Количество элементов для очистки, начиная с индекса 0</param>
    /// <remarks>
    /// Метод не выполняет никаких действий, если массив пуст или если параметр length равен 0.
    /// </remarks>
    public static void Clear<T>(T[] array, int length)
    {
        if (array.Length == 0 || length == 0) return;
        Array.Clear(array, 0, length);
    }

    /// <summary>
    /// Удаляет элемент по указанному индексу, сдвигая все последующие элементы влево.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив, из которого удаляется элемент</param>
    /// <param name="index">Индекс элемента для удаления</param>
    /// <remarks>
    /// Метод сдвигает все элементы, следующие за удаляемым, на одну позицию влево.
    /// Последний элемент массива устанавливается в значение по умолчанию.
    /// </remarks>
    public static void Cut<T>(T[] array, int index)
    {
        var length = array.Length - 1;

        if (index < length)
        {
            Array.Copy(array, index + 1, array, index, length - index);
        }

        array[length] = default!;
    }

    /// <summary>
    /// Удаляет элемент по указанному индексу в массиве с учетом заданной длины.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив, из которого удаляется элемент</param>
    /// <param name="index">Индекс элемента для удаления</param>
    /// <param name="length">Рабочая длина массива</param>
    /// <remarks>
    /// Этот метод работает с массивами, где фактически используется только часть элементов (до указанной длины).
    /// Элементы после индекса перемещаются на одну позицию влево, а элемент на позиции length устанавливается в значение по умолчанию.
    /// </remarks>
    public static void Cut<T>(T[] array, int index, int length)
    {
        if (index < length)
        {
            Array.Copy(array, index + 1, array, index, length - index);
        }

        array[length] = default!;
    }

    /// <summary>
    /// Обеспечивает необходимую емкость массива, увеличивая его при необходимости.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив, емкость которого нужно увеличить</param>
    /// <param name="capacity">Требуемая минимальная емкость</param>
    /// <param name="clear">Указывает, нужно ли очистить исходный массив после изменения размера</param>
    /// <remarks>
    /// Если текущая длина массива меньше требуемой емкости, создается новый массив с требуемой емкостью.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureCapacity<T>(ref T[] array, int capacity, bool clear = false)
    {
        if (array.Length < capacity) Resize(ref array, capacity, clear);
    }

    /// <summary>
    /// Обеспечивает необходимую емкость массива, используя пул массивов для эффективного управления памятью.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив, емкость которого нужно увеличить</param>
    /// <param name="pool">Пул массивов для аренды нового массива</param>
    /// <param name="capacity">Требуемая минимальная емкость</param>
    /// <param name="clear">Указывает, нужно ли очистить исходный массив после изменения размера</param>
    /// <remarks>
    /// Использует <see cref="ArrayPool{T}"/> для эффективного повторного использования массивов.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureCapacity<T>(ref T[] array, ArrayPool<T> pool, int capacity, bool clear = false)
    {
        if (array.Length < capacity) Resize(ref array, pool, capacity, clear);
    }

    /// <summary>
    /// Вставляет элемент по указанному индексу, автоматически увеличивая массив при необходимости.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для вставки элемента</param>
    /// <param name="index">Индекс для вставки элемента</param>
    /// <param name="element">Элемент для вставки</param>
    /// <remarks>
    /// Если индекс выходит за границы текущего массива, размер массива автоматически удваивается.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Insert<T>(ref T[] array, int index, in T element)
    {
        var arrayLength = array.Length;
        if (index >= arrayLength) Resize(ref array, arrayLength * 2);
        array[index] = element;
    }

    /// <summary>
    /// Вставляет элемент по указанному индексу, используя пул массивов для эффективного управления памятью.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для вставки элемента</param>
    /// <param name="pool">Пул массивов для аренды нового массива при необходимости</param>
    /// <param name="index">Индекс для вставки элемента</param>
    /// <param name="element">Элемент для вставки</param>
    /// <remarks>
    /// Если индекс выходит за границы текущего массива, размер массива автоматически удваивается,
    /// используя пул массивов для более эффективного управления памятью.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Insert<T>(ref T[] array, ArrayPool<T> pool, int index, in T element)
    {
        var arrayLength = array.Length;
        if (index >= arrayLength) Resize(ref array, pool, arrayLength * 2);
        array[index] = element;
    }

    /// <summary>
    /// Вставляет элемент в массив по указанному индексу, создавая массив, если он null.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для вставки элемента (может быть null)</param>
    /// <param name="index">Индекс для вставки элемента</param>
    /// <param name="element">Элемент для вставки</param>
    /// <remarks>
    /// Если массив равен null, создается новый массив с начальной емкостью 2.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InsertOrCreate<T>(ref T[]? array, int index, in T element)
    {
        array ??= GC.AllocateUninitializedArray<T>(2);
        Insert(ref array, index, in element);
    }

    /// <summary>
    /// Вставляет элемент по указанному индексу, создавая массив из пула, если он null.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для вставки элемента (может быть null)</param>
    /// <param name="pool">Пул массивов для аренды нового массива</param>
    /// <param name="index">Индекс для вставки элемента</param>
    /// <param name="element">Элемент для вставки</param>
    /// <remarks>
    /// Если массив равен null, из пула арендуется новый массив с начальной емкостью 2.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InsertOrCreate<T>(ref T[]? array, ArrayPool<T> pool, int index, in T element)
    {
        array ??= pool.Rent(2);
        Insert(ref array, pool, index, in element);
    }

    /// <summary>
    /// Удаляет первое вхождение указанного элемента из массива.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив, из которого удаляется элемент</param>
    /// <param name="item">Элемент для удаления</param>
    /// <returns>Возвращает true, если элемент был найден и удален, иначе false</returns>
    /// <remarks>
    /// Метод оптимизирует удаление в зависимости от типа элементов:
    /// - Для ссылочных типов использует метод <see cref="Cut{T}(T[], int)"/>
    /// - Для значимых типов использует <see cref="Buffer.BlockCopy"/> для более эффективного копирования
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Remove<T>(T[]? array, T item)
    {
        if (array == null) return false;

        var index = Array.IndexOf(array, item);
        if (index == -1) return false;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Cut(array, index);
        }
        else
        {
            var lastElementIndex = array.Length - 1;
            Buffer.BlockCopy(array, (index + 1) * Unsafe.SizeOf<T>(),
                array, index * Unsafe.SizeOf<T>(),
                (lastElementIndex - index) * Unsafe.SizeOf<T>());

            array[lastElementIndex] = default!;
        }

        return true;
    }

    /// <summary>
    /// Изменяет размер массива до указанной длины, сохраняя существующие элементы.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для изменения размера</param>
    /// <param name="newLength">Новая длина массива</param>
    /// <param name="clear">Указывает, нужно ли очистить исходный массив после изменения размера</param>
    /// <remarks>
    /// Если указанная длина равна 0, используется минимальная длина 2.
    /// Создается новый массив с указанной длиной, и элементы из исходного массива копируются в него.
    /// </remarks>
    public static void Resize<T>(ref T[] array, int newLength, bool clear = false)
    {
        if (newLength == 0) newLength = 2;

        var newArray = Create<T>(newLength);

        if (array.Length > 0)
        {
            Array.Copy(array, newArray, array.Length);
            if (clear) Array.Clear(array, 0, array.Length);
        }

        array = newArray;
    }

    /// <summary>
    /// Изменяет размер массива до указанной длины, используя пул массивов для эффективного управления памятью.
    /// </summary>
    /// <typeparam name="T">Тип элементов массива</typeparam>
    /// <param name="array">Массив для изменения размера</param>
    /// <param name="pool">Пул массивов для аренды нового массива</param>
    /// <param name="newLength">Новая длина массива</param>
    /// <param name="clear">Указывает, нужно ли очистить исходный массив перед возвратом в пул</param>
    /// <remarks>
    /// Если указанная длина равна 0, используется минимальная длина 2.
    /// Арендуется новый массив из пула с указанной длиной, элементы копируются, и старый массив возвращается в пул.
    /// </remarks>
    public static void Resize<T>(ref T[] array, ArrayPool<T> pool, int newLength, bool clear = false)
    {
        if (newLength == 0) newLength = 2;

        var newArray = pool.Rent(newLength);

        if (array.Length > 0)
        {
            Array.Copy(array, newArray, array.Length);
            pool.Return(array, clear);
        }

        array = newArray;
    }
}