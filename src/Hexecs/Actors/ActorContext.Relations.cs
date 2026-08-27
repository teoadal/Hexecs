using Hexecs.Actors.Relations;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private IActorRelationPool?[] _relationPools;
#if NET9_0_OR_GREATER
    private readonly Lock _relationPoolLock = new Lock();
#else
    private readonly object _relationPoolLock = new object();
#endif

    /// <summary>
    /// Добавляет отношение указанного типа между двумя актёрами.
    /// </summary>
    /// <typeparam name="T">Тип отношения. Должен быть структурой.</typeparam>
    /// <param name="subject">Идентификатор первого актёра (субъекта).</param>
    /// <param name="relative">Идентификатор второго актёра (относительного).</param>
    /// <param name="relation">Данные отношения.</param>
    /// <returns>Ссылка на добавленное отношение.</returns>
    /// <exception cref="Exception">Вызывается, если отношение между указанными актёрами уже существует.</exception>
    public ref T AddRelation<T>(ActorId subject, ActorId relative, in T relation)
        where T : struct
    {
        ActorRelationPool<T> relationPool = GetOrCreateRelationPool<T>();
        ref T addResult = ref relationPool.Add(subject, relative, in relation);

        ushort relationId = ActorRelationType<T>.Id;

        ref ActorRelationComponent subjectRelations = ref GetOrAddComponent(subject, ActorRelationComponent.Create);
        subjectRelations.TryAdd(relationId);

        ref ActorRelationComponent relativeRelations = ref GetOrAddComponent(relative, ActorRelationComponent.Create);
        relativeRelations.TryAdd(relationId);

        return ref addResult;
    }

    /// <summary>
    /// Получает отношение указанного типа между двумя актёрами.
    /// </summary>
    /// <typeparam name="T">Тип отношения. Должен быть структурой.</typeparam>
    /// <param name="subject">Идентификатор первого актёра (субъекта).</param>
    /// <param name="relative">Идентификатор второго актёра (относительного).</param>
    /// <returns>Ссылка на найденное отношение.</returns>
    /// <exception cref="Exception">Вызывается, если отношение не найдено.</exception>
    public ref T GetRelation<T>(ActorId subject, ActorId relative) where T : struct
    {
        ActorRelationPool<T>? pool = GetRelationPool<T>();

        if (pool == null)
        {
            ActorError.RelationNotFound<T>(subject, relative);
        }

        return ref pool.Get(subject, relative);
    }

    /// <summary>
    /// Проверяет наличие отношения указанного типа между двумя актёрами.
    /// </summary>
    /// <typeparam name="T">Тип отношения. Должен быть структурой.</typeparam>
    /// <param name="subject">Идентификатор первого актёра (субъекта).</param>
    /// <param name="relative">Идентификатор второго актёра (относительного).</param>
    /// <returns>True, если отношение существует, иначе false.</returns>
    public bool HasRelation<T>(ActorId subject, ActorId relative) where T : struct
    {
        ActorRelationPool<T>? pool = GetRelationPool<T>();

        return pool != null && pool.Has(subject, relative);
    }

    /// <summary>
    /// Получает перечислитель для всех отношений указанного типа, связанных с данным актёром.
    /// </summary>
    /// <typeparam name="T">Тип отношения. Должен быть структурой.</typeparam>
    /// <param name="subject">Идентификатор актёра (субъекта).</param>
    /// <returns>Перечислитель отношений.</returns>
    public ActorRelationEnumerator<T> Relations<T>(ActorId subject) where T : struct
    {
        ActorRelationPool<T>? pool = GetRelationPool<T>();

        // ReSharper disable once MergeConditionalExpression
        return pool == null
            ? ActorRelationEnumerator<T>.Empty
            : pool.GetRelations(subject);
    }

    /// <summary>
    /// Удаляет отношение указанного типа между двумя актёрами.
    /// </summary>
    /// <typeparam name="T">Тип отношения. Должен быть структурой.</typeparam>
    /// <param name="subject">Идентификатор первого актёра (субъекта).</param>
    /// <param name="relative">Идентификатор второго актёра (относительного).</param>
    /// <returns>True, если отношение было успешно удалено, иначе false.</returns>
    public bool RemoveRelation<T>(ActorId subject, ActorId relative) where T : struct
    {
        return RemoveRelation<T>(subject, relative, out _);
    }

    /// <summary>
    /// Удаляет отношение указанного типа между двумя актёрами и возвращает удаленное отношение.
    /// </summary>
    /// <typeparam name="T">Тип отношения. Должен быть структурой.</typeparam>
    /// <param name="subject">Идентификатор первого актёра (субъекта).</param>
    /// <param name="relative">Идентификатор второго актёра (относительного).</param>
    /// <param name="relation">Удаленное отношение.</param>
    /// <returns>True, если отношение было успешно удалено, иначе false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRelation<T>(ActorId subject, ActorId relative, out T relation) where T : struct
    {
        ActorRelationPool<T>? pool = GetRelationPool<T>();

        if (pool == null)
        {
            relation = default;

            return false;
        }

        bool removeResult = pool.Remove(subject, relative, out relation);

        if (!removeResult)
        {
            relation = default;

            return false;
        }

        ushort relationId = ActorRelationType<T>.Id;

        if (pool.Count(subject) == 0)
        {
            ref ActorRelationComponent subjectRelations = ref GetComponent<ActorRelationComponent>(subject);
            subjectRelations.Remove(relationId);
        }

        if (pool.Count(relative) == 0)
        {
            ref ActorRelationComponent relativeRelations = ref GetComponent<ActorRelationComponent>(relative);
            relativeRelations.Remove(relationId);
        }

        return true;
    }

    private ActorRelationPool<T>? GetRelationPool<T>() where T : struct
    {
        ushort id = ActorRelationType<T>.Id;

        if (id >= _relationPools.Length)
        {
            return null;
        }

        IActorRelationPool? pool = _relationPools[id];

        return pool == null
            ? null
            : Unsafe.As<ActorRelationPool<T>>(pool);
    }

    private ActorRelationPool<T> GetOrCreateRelationPool<T>() where T : struct
    {
        ushort relationId = ActorRelationType<T>.Id;

        if (relationId < _relationPools.Length)
        {
            IActorRelationPool? existsPool = _relationPools[relationId];

            if (existsPool != null)
            {
                return Unsafe.As<ActorRelationPool<T>>(existsPool);
            }
        }

#if NET9_0_OR_GREATER
        using (_relationPoolLock.EnterScope())
#else
        lock (_relationPoolLock)
#endif
        {
            ArrayUtils.EnsureCapacity(ref _relationPools, relationId);
            ref IActorRelationPool? pool = ref _relationPools[relationId];
            pool ??= new ActorRelationPool<T>(this);

            return Unsafe.As<ActorRelationPool<T>>(pool);
        }
    }
}
