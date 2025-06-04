namespace Hexecs.Utils;

/// <summary>
/// Реализация интерфейса IEqualityComparer для сравнения объектов по ссылке.
/// Класс использует метод ReferenceEquals для сравнения, что означает,
/// что объекты считаются равными только если они являются одной и той же ссылкой в памяти.
/// </summary>
/// <typeparam name="T">Тип объекта для сравнения, должен быть ссылочным типом.</typeparam>
internal sealed class ReferenceComparer<T> : IEqualityComparer<T>
    where T : class
{
    /// <summary>
    /// Статический экземпляр компаратора для использования в качестве синглтона.
    /// Позволяет избежать создания множества экземпляров класса.
    /// </summary>
    public static readonly IEqualityComparer<T> Instance = new ReferenceComparer<T>();

    /// <summary>
    /// Приватный конструктор предотвращает создание экземпляров класса извне,
    /// обеспечивая шаблон синглтон.
    /// </summary>
    private ReferenceComparer()
    {
    }

    /// <summary>
    /// Определяет, равны ли указанные объекты, сравнивая их по ссылке.
    /// </summary>
    /// <param name="x">Первый сравниваемый объект.</param>
    /// <param name="y">Второй сравниваемый объект.</param>
    /// <returns>Возвращает true, если объекты ссылаются на один и тот же экземпляр; в противном случае — false.</returns>
    public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

    /// <summary>
    /// Возвращает хеш-код для указанного объекта, используя <see cref="RuntimeHelpers.GetHashCode(object?)"/>
    /// </summary>
    /// <param name="obj">Объект, для которого требуется получить хеш-код.</param>
    /// <returns>Хеш-код, соответствующий идентичности объекта.</returns>
    public int GetHashCode([DisallowNull] T obj) => RuntimeHelpers.GetHashCode(obj);
}