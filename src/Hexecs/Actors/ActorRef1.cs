using Hexecs.Actors.Development;
using Hexecs.Actors.Relations;
using Hexecs.Assets;

namespace Hexecs.Actors;

/// <summary>
/// Структура, представляющая собой ссылку на актёра с компонентом типа <typeparamref name="T1"/>.
/// </summary>
/// <typeparam name="T1">Тип компонента актёра</typeparam>
/// <remarks>
/// Это легковесный объект, который содержит идентификатор актёра, ссылку на контекст
/// и ссылку на компонент указанного типа, что позволяет эффективно взаимодействовать с актёром.
/// </remarks>
[DebuggerDisplay("{ToString()}")]
[DebuggerTypeProxy(typeof(ActorRefDebugProxy<>))]
public readonly ref struct ActorRef<T1>
    where T1 : struct, IActorComponent
{
    /// <summary>
    /// Пустая ссылка на актёра, используемая по умолчанию.
    /// </summary>
    public static ActorRef<T1> Empty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(null!, Actor.EmptyId, ref Unsafe.NullRef<T1>());
    }

    /// <summary>
    /// Проверяет, существует ли актёр в системе.
    /// </summary>
    public bool Alive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context?.ActorAlive(Id) ?? false;
    }

    /// <summary>
    /// Ссылка на первый компонент актёра.
    /// </summary>
    public ref T1 Component1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _component1;
    }

    /// <summary>
    /// Проверяет, является ли ссылка на актёра пустой (без контекста).
    /// </summary>
    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Context == null;
    }

    /// <summary>
    /// Контекст актёра, управляющий его жизненным циклом и взаимодействием с компонентами.
    /// </summary>
    public readonly ActorContext Context;

    /// <summary>
    /// Ссылка на первый компонент актёра.
    /// </summary>
    private readonly ref T1 _component1;

    /// <summary>
    /// Уникальный идентификатор актёра.
    /// </summary>
    public readonly uint Id;

    /// <summary>
    /// Внутренний конструктор для создания ссылки на актёра.
    /// </summary>
    /// <param name="context">Контекст актёра.</param>
    /// <param name="id">Идентификатор актёра.</param>
    /// <param name="component1">Ссылка на первый компонент актёра.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ActorRef(ActorContext context, uint id, ref T1 component1)
    {
        Context = context;
        _component1 = ref component1;
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
    /// Преобразует ссылку на актёра в типизированного актёра с указанным компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Типизированный актёр с указанным компонентом.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден или не содержит указанный компонент.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor<T> As<T>() where T : struct, IActorComponent => Context.GetActor<T>(Id);

    /// <summary>
    /// Преобразует ссылку на актёра в ссылку на типизированного актёра.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <returns>Ссылка на типизированного актёра.</returns>
    /// <exception cref="Exception">Выбрасывается, если актёр не найден или не содержит указанный компонент.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ActorRef<T> AsRef<T>() where T : struct, IActorComponent => Context.GetActorRef<T>(Id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor<T1> AsActor() => IsEmpty 
        ? Actor<T1>.Empty 
        : new Actor<T1>(Context, Id);

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
    /// Проверяет, можно ли преобразовать ссылку на актёра в типизированного актёра с указанным компонентом.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="actor">Результирующий типизированный актёр.</param>
    /// <returns>Возвращает true, если преобразование успешно; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is<T>(out Actor<T> actor) where T : struct, IActorComponent => Context.TryGetActor(Id, out actor);

    /// <summary>
    /// Проверяет, можно ли преобразовать ссылку на актёра в ссылку на типизированного актёра.
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
    /// Возвращает строковое представление ссылки на актёра.
    /// </summary>
    /// <returns>Строковое представление ссылки на актёра.</returns>
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
    /// Проверяет равенство между двумя ссылками на актёров.
    /// </summary>
    /// <param name="other">Ссылка на актёра для сравнения.</param>
    /// <returns>Возвращает true, если ссылки на актёров равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ActorRef<T1> other) => Id == other.Id && ReferenceEquals(Context, other.Context);

    /// <summary>
    /// Проверяет равенство с другим объектом.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>Возвращает true, если объекты равны; иначе false.</returns>
    public override bool Equals(object? obj) => obj switch
    {
        Actor<T1> other => other.Id == Id,
        Actor actor => actor.IsRef<T1>(out var expected) && Equals(expected),
        _ => false
    };

    /// <summary>
    /// Возвращает хеш-код для ссылки на актёра.
    /// </summary>
    /// <returns>Хеш-код ссылки на актёра.</returns>
    public override int GetHashCode() => HashCode.Combine(Id);

    /// <summary>
    /// Сравнивает две ссылки на актёров на равенство.
    /// </summary>
    /// <param name="left">Левая ссылка на актёра.</param>
    /// <param name="right">Правая ссылка на актёра.</param>
    /// <returns>Возвращает true, если ссылки на актёров равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in ActorRef<T1> left, in ActorRef<T1> right) => left.Equals(right);

    /// <summary>
    /// Сравнивает две ссылки на актёров на неравенство.
    /// </summary>
    /// <param name="left">Левая ссылка на актёра.</param>
    /// <param name="right">Правая ссылка на актёра.</param>
    /// <returns>Возвращает true, если ссылки на актёров не равны; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in ActorRef<T1> left, in ActorRef<T1> right) => !left.Equals(right);

    #endregion

    #region Implicit

    /// <summary>
    /// Неявное преобразование ссылки на актёра в bool.
    /// </summary>
    /// <param name="actor">Ссылка на актёра для преобразования.</param>
    /// <returns>Возвращает true, если ссылка на актёра не пустая; иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in ActorRef<T1> actor) => !actor.IsEmpty;

    /// <summary>
    /// Неявное преобразование ссылки на актёра в ActorId.
    /// </summary>
    /// <param name="actor">Ссылка на актёра для преобразования.</param>
    /// <returns>Идентификатор актёра.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorId(in ActorRef<T1> actor) => new(actor.Id);

    /// <summary>
    /// Неявное преобразование ссылки на актёра в типизированный идентификатор актёра.
    /// </summary>
    /// <param name="actor">Ссылка на актёра для преобразования.</param>
    /// <returns>Типизированный идентификатор актёра.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ActorId<T1>(in ActorRef<T1> actor) => new(actor.Id);

    /// <summary>
    /// Неявное преобразование ссылки на актёра в актёра.
    /// </summary>
    /// <param name="actor">Ссылка на актёра для преобразования.</param>
    /// <returns>Актёр.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Actor(in ActorRef<T1> actor) => new(actor.Context, actor.Id);

    /// <summary>
    /// Неявное преобразование ссылки на актёра в типизированного актёра.
    /// </summary>
    /// <param name="actor">Ссылка на актёра для преобразования.</param>
    /// <returns>Типизированный актёр.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Actor<T1>(in ActorRef<T1> actor) => new(actor.Context, actor.Id);

    #endregion
}