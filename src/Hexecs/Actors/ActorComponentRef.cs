using Hexecs.Actors.Components;

namespace Hexecs.Actors;

/// <summary>
/// Представляет собой ссылку на компонент <typeparamref name="T"/> актёра, управляемый пулом.
/// Эта структура обеспечивает безопасный способ доступа к компонентам, позволяя проверять, действительна ли ссылка.
/// </summary>
/// <typeparam name="T">Тип компонента актёра. Должен быть структурой, реализующей <see cref="IActorComponent"/>.</typeparam>
public readonly struct ActorComponentRef<T>
    where T : struct, IActorComponent
{
    /// <summary>
    /// Получает пустую ссылку на компонент актёра.
    /// Эта ссылка указывает на отсутствие или недействительность компонента.
    /// </summary>
    public static ActorComponentRef<T> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new();
    }

    /// <summary>
    /// Получает значение, указывающее, является ли текущая ссылка на компонент актёра пустой.
    /// </summary>
    /// <value><c>true</c> если ссылка пуста; в противном случае, <c>false</c>.</value>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pool == null;
    }

    private readonly int _index;
    private readonly ActorComponentPool<T> _pool;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ActorComponentRef(ActorComponentPool<T> pool, int index)
    {
        _pool = pool;
        _index = index;
    }

    /// <summary>
    /// Разворачивает ссылку и возвращает управляемую ссылку на сам компонент актёра.
    /// </summary>
    /// <remarks>
    /// Перед вызовом этого метода необходимо убедиться, что ссылка не является пустой (проверив свойство <see cref="IsEmpty"/>).
    /// Вызов этого метода для пустой ссылки (когда <see cref="IsEmpty"/> имеет значение <c>true</c>) приведёт к ошибке выполнения, 
    /// так как внутренняя попытка доступа к компоненту через неинициализированный пул вызовет исключение.
    /// </remarks>
    /// <returns>Управляемая ссылка на компонент актёра <typeparamref name="T"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Unwrap() => ref _pool.GetByIndex(_index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorComponentRef<T> componentRef) => !componentRef.IsEmpty;
}