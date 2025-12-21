namespace Hexecs.Actors;

/// <summary>
/// Класс, представляющий ограничения для актёров, позволяющий фильтровать актёры 
/// на основе наличия или отсутствия определенных компонентов.
/// </summary>
public sealed partial class ActorConstraint : IEquatable<ActorConstraint>, IDisposable
{
    /// <summary>
    /// Создает построитель ограничений с исключением указанного компонента.
    /// </summary>
    /// <typeparam name="T1">Тип компонента, который должен отсутствовать у актёра</typeparam>
    /// <param name="context">Контекст актёров</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Exclude<T1>(ActorContext context) where T1 : struct, IActorComponent
    {
        return new Builder(context).Exclude<T1>();
    }

    /// <summary>
    /// Создает построитель ограничений с включением указанного компонента.
    /// </summary>
    /// <typeparam name="T1">Тип компонента, который должен присутствовать у актёра</typeparam>
    /// <param name="context">Контекст актёров</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Include<T1>(ActorContext context) where T1 : struct, IActorComponent
    {
        return new Builder(context).Include<T1>();
    }

    /// <summary>
    /// Создает построитель ограничений с включением двух указанных компонентов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен присутствовать у актёра</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен присутствовать у актёра</typeparam>
    /// <param name="context">Контекст актёров</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Include<T1, T2>(ActorContext context)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        return new Builder(context)
            .Include<T1>()
            .Include<T2>();
    }

    /// <summary>
    /// Создает построитель ограничений с включением трех указанных компонентов.
    /// </summary>
    /// <typeparam name="T1">Первый тип компонента, который должен присутствовать у актёра</typeparam>
    /// <typeparam name="T2">Второй тип компонента, который должен присутствовать у актёра</typeparam>
    /// <typeparam name="T3">Третий тип компонента, который должен присутствовать у актёра</typeparam>
    /// <param name="context">Контекст актёров</param>
    /// <returns>Построитель ограничений</returns>
    public static Builder Include<T1, T2, T3>(ActorContext context)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        return new Builder(context)
            .Include<T1>()
            .Include<T2>()
            .Include<T3>();
    }

    /// <summary>
    /// Событие, вызываемое при добавлении актёра, соответствующего ограничениям.
    /// </summary>
    public event Action<uint>? Added;

    /// <summary>
    /// Событие, вызываемое при удалении актёра, который ранее соответствовал ограничениям.
    /// </summary>
    public event Action<uint>? Removing;

    private readonly int _contextId;
    private readonly int _hash;
    private readonly Subscription[] _subscriptions;

    private ActorConstraint(int contextId, int hash, Subscription[] subscriptions)
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Subscribe(this);
        }

        _contextId = contextId;
        _hash = hash;
        _subscriptions = subscriptions;
    }

    /// <summary>
    /// Проверяет, применимо ли данное ограничение к актёру с указанным идентификатором.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра для проверки</param>
    /// <returns>Возвращает true, если актёр соответствует всем ограничениям; иначе false</returns>
    public bool Applicable(uint actorId)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var subscription in _subscriptions)
        {
            if (!subscription.Check(actorId)) return false;
        }

        return true;
    }

    private void OnExclude(uint actorId) => Removing?.Invoke(actorId);

    private void OnInclude(uint actorId)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var subscription in _subscriptions)
        {
            if (!subscription.Check(actorId)) return;
        }

        Added?.Invoke(actorId);
    }

    #region Equality

    /// <summary>
    /// Определяет, равен ли указанный объект ActorConstraint текущему объекту.
    /// </summary>
    /// <param name="other">Объект ActorConstraint для сравнения с текущим объектом</param>
    /// <returns>Возвращает true, если указанный объект равен текущему объекту; иначе false</returns>
    public bool Equals(ActorConstraint? other)
    {
        if (other == null || _contextId != other._contextId) return false;

        var otherSubscriptions = other._subscriptions;

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < _subscriptions.Length; i++)
        {
            if (!_subscriptions[i].Equals(otherSubscriptions[i])) return false;
        }

        return true;
    }

    /// <summary>
    /// Определяет, равен ли указанный объект текущему объекту.
    /// </summary>
    /// <param name="obj">Объект для сравнения с текущим объектом</param>
    /// <returns>Возвращает true, если указанный объект равен текущему объекту; иначе false</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ActorConstraint other && Equals(other);
    }

    /// <summary>
    /// Возвращает хеш-код для текущего объекта.
    /// </summary>
    /// <returns>Хеш-код для текущего объекта</returns>
    public override int GetHashCode() => _hash;

    #endregion

    /// <summary>
    /// Освобождает ресурсы, используемые экземпляром ActorConstraint.
    /// </summary>
    void IDisposable.Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Unsubscribe(this);
        }
    }
}