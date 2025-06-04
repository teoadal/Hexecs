namespace Hexecs.Collections;

/// <summary>
/// Структура-перечислитель для массива элементов типа T.
/// </summary>
/// <typeparam name="T">Тип элементов массива.</typeparam>
public struct ArrayEnumerator<T> : IEnumerator<T>
{
    /// <summary>
    /// Возвращает пустой перечислитель массива.
    /// </summary>
    public static ArrayEnumerator<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new();
    }

    /// <summary>
    /// Получает текущий элемент в последовательности по ссылке.
    /// </summary>
    public readonly ref T Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _array[_index];
    }

    private int _index;
    private readonly T[] _array;
    private readonly int _length;

    #region Constructors

    /// <summary>
    /// Инициализирует новый экземпляр пустого перечислителя массива.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArrayEnumerator()
    {
        _index = -1;
        _array = [];
        _length = 0;
    }

    /// <summary>
    /// Инициализирует новый экземпляр перечислителя для указанного массива.
    /// </summary>
    /// <param name="array">Массив для перечисления.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArrayEnumerator(T[] array)
    {
        _index = -1;
        _array = array;
        _length = array.Length;
    }

    /// <summary>
    /// Инициализирует новый экземпляр перечислителя для указанного массива с заданной длиной.
    /// </summary>
    /// <param name="array">Массив для перечисления.</param>
    /// <param name="length">Количество элементов для перечисления.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArrayEnumerator(T[] array, int length)
    {
        _index = -1;
        _array = array;
        _length = length;
    }

    #endregion

    /// <summary>
    /// Перемещает перечислитель к следующему элементу последовательности.
    /// </summary>
    /// <returns>Возвращает true, если перечислитель успешно перемещен к следующему элементу; false, если перечислитель достиг конца последовательности.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() => ++_index < _length;

    /// <summary>
    /// Устанавливает перечислитель в его начальное положение.
    /// </summary>
    public void Reset()
    {
        _index = -1;
    }

    #region Interfaces

    readonly T IEnumerator<T>.Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Current;
    }

    readonly void IDisposable.Dispose()
    {
    }

    object IEnumerator.Current => Current!;

    #endregion
}