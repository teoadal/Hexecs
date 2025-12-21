using Hexecs.Actors.Development;
using Hexecs.Actors.Relations;
using Hexecs.Assets;

namespace Hexecs.Actors;

/// <summary>
/// Структура, представляющая собой типизированного актёра в системе с компонентом типа <typeparamref name="T1"/>.
/// </summary>
/// <typeparam name="T1">Тип компонента актёра</typeparam>
/// <remarks>
/// Актёр - это базовый объект, который содержит идентификатор и ссылку на контекст,
/// позволяющий взаимодействовать с компонентами и отношениями.
/// </remarks>
[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(ActorDebugProxy<>))]
public readonly struct Actor<T1> : IEquatable<Actor<T1>>
    where T1 : struct, IActorComponent
{
    /// <summary>
    /// Константа, представляющая собой пустой идентификатор актёра.
    /// </summary>
    private const uint EmptyId = 0;

    /// <summary>
    /// Контекст актёра, управляющий его жизненным циклом и взаимодействием с компонентами.
    /// </summary>
    public readonly ActorContext Context;

    /// <summary>
    /// Уникальный идентификатор актёра.
    /// </summary>
    public readonly uint Id;
    
    /// <summary>
    /// Пустой актёр, используемый по умолчанию.
    /// </summary>
    public static Actor<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, EmptyId);
    }

    /// <summary>
    /// Проверяет, существует ли актёр в системе.
    /// </summary>
    public bool Alive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context != null && Context.ActorAlive(Id);
    }

    /// <summary>
    /// Предоставляет доступ к первому компоненту актёра.
    /// </summary>
    public ref T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Context.GetComponent<T1>(Id);
    }

    /// <summary>
    /// Проверяет, является ли актёр пустым (без контекста).
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context == null;
    }

    /// <summary>
    /// Внутренний конструктор для создания актёра.
    /// </summary>
    /// <param name="context">Контекст актёра.</param>
    /// <param name="id">Идентификатор актёра.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Actor(ActorContext context, uint id)
    {
        Context = context;
        Id = id;
    }

    /// <summary>
    /// Добавляет компонент к актёру.
    /// </summary>
    /// <typeparam name="T">Тип компонента, должен быть структурой и реализовывать IActorComponent.</typeparam>
    /// <param name="component">Компонент для добавления.</param>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден в контексте или компонент уже существует.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>(in T component) where T : struct, IActorComponent => Context.AddComponent(Id, in component);

    /// <summary>
    /// Добавляет дочернего актёра к текущему.
    /// </summary>
    /// <param name="child">Дочерний актёр.</param>
    /// <exception cref="Exception">Выбрасывается, если один из актёров не найден или дочерний актёр уже добавлен.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChild(in Actor child) => Context.AddChild(Id, child.Id);

    /// <summary>
    /// Добавляет отношение между текущим актёром и указанным родственным актёром.
    /// </summary>
    /// <typeparam name="T">Тип отношения.</typeparam>
    /// <param name="relative">Родственный актёр.</param>
    /// <param name="relation">Отношение для добавления.</param>
    /// <returns>Ссылка на добавленное отношение.</returns>
    /// <exception cref="Exception">Выбрасывается, если один из актёров не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T AddRelation<T>(in Actor relative, in T relation) where T : struct
    {
        return ref Context.AddRelation(Id, relative.Id, in relation);
    }

    /// <summary>
    /// Преобразует актёра в типизированного актёра с указанным компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Типизированный актёр с указанным компонентом.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден или не содержит указанный компонент.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor<T> As<T>() where T : struct, IActorComponent => Context.GetActor<T>(Id);

    /// <summary>
    /// Преобразует актёра в ссылку на типизированного актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Ссылка на типизированного актёра.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден или не содержит указанный компонент.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef<T> AsRef<T>() where T : struct, IActorComponent => Context.GetActorRef<T>(Id);

    /// <summary>
    /// Возвращает перечислитель дочерних актёров.
    /// </summary>
    /// <returns>Перечислитель дочерних актёров.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorContext.ChildrenEnumerator Children() => Context.Children(Id);

    /// <summary>
    /// Уничтожает актёра.
    /// </summary>
    /// <returns>Возвращает true, если актёр был успешно уничтожен; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Destroy() => Context.DestroyActor(Id);

    /// <summary>
    /// Получает компонент актёра указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Ссылка на компонент.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден или не содержит указанный компонент.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<T>() where T : struct, IActorComponent => ref Context.GetComponent<T>(Id);

    /// <summary>
    /// Получает ассет, связанный с актёром.
    /// </summary>
    /// <returns>Ассет, связанный с актёром.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден или с ним не связан ассет.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Asset GetAsset() => Context.GetBoundAsset(Id);

    /// <summary>
    /// Получает отношение между текущим актёром и указанным родственным актёром.
    /// </summary>
    /// <typeparam name="T">Тип отношения.</typeparam>
    /// <param name="relative">Родственный актёр.</param>
    /// <returns>Ссылка на отношение.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден или отношение не существует.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRelation<T>(in Actor relative) where T : struct => ref Context.GetRelation<T>(Id, relative.Id);

    /// <summary>
    /// Проверяет, содержит ли актёр компонент указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Возвращает true, если актёр содержит компонент; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : struct, IActorComponent => Context.HasComponent<T>(Id);

    /// <summary>
    /// Проверяет, существует ли отношение между текущим актёром и указанным родственным актёром.
    /// </summary>
    /// <typeparam name="T">Тип отношения.</typeparam>
    /// <param name="relative">Родственный актёр.</param>
    /// <returns>Возвращает true, если отношение существует; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если один из актёров не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasRelation<T>(in Actor relative) where T : struct => Context.HasRelation<T>(Id, relative.Id);

    /// <summary>
    /// Проверяет, можно ли преобразовать актёра в типизированного актёра с указанным компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="actor">Результирующий типизированный актёр.</param>
    /// <returns>Возвращает true, если преобразование успешно; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is<T>(out Actor<T> actor) where T : struct, IActorComponent => Context.TryGetActor(Id, out actor);

    /// <summary>
    /// Проверяет, можно ли преобразовать актёра в ссылку на типизированного актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="actor">Результирующая ссылка на типизированного актёра.</param>
    /// <returns>Возвращает true, если преобразование успешно; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRef<T>(out ActorRef<T> actor) where T : struct, IActorComponent
    {
        return Context.TryGetActorRef(Id, out actor);
    }

    /// <summary>
    /// Возвращает перечислитель отношений указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип отношения.</typeparam>
    /// <returns>Перечислитель отношений.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRelationEnumerator<T> Relations<T>() where T : struct => Context.Relations<T>(Id);

    /// <summary>
    /// Удаляет компонент указанного типа из актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Возвращает true, если компонент был успешно удален; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove<T>() where T : struct, IActorComponent => Context.RemoveComponent<T>(Id);

    /// <summary>
    /// Удаляет компонент указанного типа из актёра и возвращает его значение.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="component">Удаленный компонент.</param>
    /// <returns>Возвращает true, если компонент был успешно удален; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove<T>(out T component) where T : struct, IActorComponent
    {
        return Context.RemoveComponent(Id, out component);
    }

    /// <summary>
    /// Удаляет дочернего актёра из текущего.
    /// </summary>
    /// <param name="child">Дочерний актёр для удаления.</param>
    /// <returns>Возвращает true, если дочерний актёр был успешно удален; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если один из актёров не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveChild(in Actor child) => Context.RemoveChild(Id, child.Id);

    /// <summary>
    /// Удаляет отношение указанного типа между текущим актёром и указанным родственным актёром.
    /// </summary>
    /// <typeparam name="T">Тип отношения.</typeparam>
    /// <param name="relative">Родственный актёр.</param>
    /// <returns>Возвращает true, если отношение было успешно удалено; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если один из актёров не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRelation<T>(in Actor relative) where T : struct => Context.RemoveRelation<T>(Id, relative.Id);

    /// <summary>
    /// Удаляет отношение указанного типа между текущим актёром и указанным родственным актёром и возвращает его значение.
    /// </summary>
    /// <typeparam name="T">Тип отношения.</typeparam>
    /// <param name="relative">Родственный актёр.</param>
    /// <param name="relation">Удаленное отношение.</param>
    /// <returns>Возвращает true, если отношение было успешно удалено; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если один из актёров не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRelation<T>(in Actor relative, out T relation) where T : struct
    {
        return Context.RemoveRelation(Id, relative.Id, out relation);
    }

    /// <summary>
    /// Возвращает строковое представление актёра.
    /// </summary>
    /// <returns>Строковое представление актёра.</returns>
    public override string ToString() => Context == null
        ? StringUtils.EmptyValue
        : Context.GetDescription(Id);

    /// <summary>
    /// Пытается добавить компонент к актёру, если он еще не существует.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="component">Компонент для добавления.</param>
    /// <returns>Возвращает true, если компонент был успешно добавлен; иначе false, если он уже существует.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd<T>(in T component) where T : struct, IActorComponent => Context.TryAdd(Id, in component);

    /// <summary>
    /// Пытается получить ассет, связанный с актёром.
    /// </summary>
    /// <param name="asset">Результирующий ассет.</param>
    /// <returns>Возвращает true, если ассет был успешно получен; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetAsset(out Asset asset) => Context.TryGetBoundAsset(Id, out asset);

    /// <summary>
    /// Пытается получить ссылку на компонент актёра указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Ссылка на компонент или на пустой компонент, если он не существует.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T TryGetRef<T>() where T : struct, IActorComponent => ref Context.TryGetComponentRef<T>(Id);

    /// <summary>
    /// Пытается получить родительского актёра для текущего.
    /// </summary>
    /// <param name="parent">Результирующий родительский актёр.</param>
    /// <returns>Возвращает true, если родительский актёр существует; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetParent(out Actor parent) => Context.TryGetParent(Id, out parent);

    /// <summary>
    /// Обновляет существующий компонент или создает новый, если он не существует.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="component">Компонент для обновления.</param>
    /// <param name="createIfNotExists">Возвращает true, чтобы создать компонент, если он не существует; иначе false.</param>
    /// <returns>Возвращает true, если компонент был успешно обновлен или создан; иначе false.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Update<T>(in T component, bool createIfNotExists = true) where T : struct, IActorComponent
    {
        return Context.UpdateComponent(Id, in component, createIfNotExists);
    }

    #region Equality

    /// <summary>
    /// Проверяет равенство между двумя актёрами.
    /// </summary>
    /// <param name="other">Актёр для сравнения.</param>
    /// <returns>Возвращает true, если актёры равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Actor<T1> other) => Id == other.Id && ReferenceEquals(Context, other.Context);

    /// <summary>
    /// Проверяет равенство с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>Возвращает true, если объекты равны; иначе false.</returns>
    public override bool Equals(object? obj) => obj switch
    {
        Actor<T1> other => Equals(other),
        Actor actor => actor.Is<T1>(out var expected) && Equals(expected),
        _ => false
    };

    /// <summary>
    /// Возвращает хеш-код для актёра.
    /// </summary>
    /// <returns>Хеш-код актёра.</returns>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// Сравнивает двух актёров на равенство.
    /// </summary>
    /// <param name="left">Левый актёр.</param>
    /// <param name="right">Правый актёр.</param>
    /// <returns>Возвращает true, если актёры равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Actor<T1> left, in Actor<T1> right) => left.Equals(right);

    /// <summary>
    /// Сравнивает двух актёров на неравенство.
    /// </summary>
    /// <param name="left">Левый актёр.</param>
    /// <param name="right">Правый актёр.</param>
    /// <returns>Возвращает true, если актёры не равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Actor<T1> left, in Actor<T1> right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование актёра в bool.
    /// </summary>
    /// <param name="actor">Актёр для преобразования.</param>
    /// <returns>Возвращает true, если актёр не пустой; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in Actor<T1> actor) => !actor.IsEmpty;

    /// <summary>
    /// Неявное преобразование актёра в <see cref="ActorId"/>.
    /// </summary>
    /// <param name="actor">Актёр для преобразования.</param>
    /// <returns>Идентификатор актёра.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorId(in Actor<T1> actor) => new(actor.Id);

    /// <summary>
    /// Неявное преобразование актёра в типизированный идентификатор <see cref="ActorId{T1}"/>.
    /// </summary>
    /// <param name="actor">Актёр для преобразования.</param>
    /// <returns>Типизированный идентификатор актёра.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorId<T1>(in Actor<T1> actor) => new(actor.Id);

    /// <summary>
    /// Неявное преобразование типизированного актёра в базовый актёр.
    /// </summary>
    /// <param name="actor">Типизированный актёр для преобразования.</param>
    /// <returns>Базовый актёр.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Actor(in Actor<T1> actor) => new(actor.Context, actor.Id);

    #endregion
}