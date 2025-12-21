using Hexecs.Actors.Components;
using Hexecs.Assets;
using Hexecs.Worlds;

namespace Hexecs.Actors;

/// <summary>
/// Статический класс, предоставляющий служебные методы для работы с компонентами акторов.
/// Предоставляет низкоуровневый доступ к компонентам и облегчает маршаллинг между акторами и их компонентами.
/// </summary>
public static class ActorMarshal
{
    /// <summary>
    /// Удаляет данные компонентов (не сами компоненты).
    /// Фактически заменяет значение компонентов на default.
    /// </summary>
    /// <param name="context">Контекст компонентов.</param>
    /// <typeparam name="T">Тип компонента</typeparam>
    public static void ClearComponentsData<T>(ActorContext context)
        where T : struct, IActorComponent
    {
        var pool = context.GetComponentPool<T>();
        pool?.GetValues().Clear();
    }
    
    /// <summary>
    /// Получает идентификатор типа компонента.
    /// </summary>
    /// <typeparam name="T">Тип компонента актёра.</typeparam>
    /// <returns>Идентификатор типа компонента.</returns>
    public static ushort GetComponentId<T>() where T : struct, IActorComponent => ActorComponentType<T>.Id;

    /// <summary>
    /// Получает идентификатор типа компонента по его типу.
    /// </summary>
    /// <param name="componentType">Тип компонента.</param>
    /// <returns>Идентификатор типа компонента.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если переданный тип не является компонентом актёра.</exception>
    public static ushort GetComponentId(Type componentType)
    {
        if (componentType.IsAssignableTo(typeof(IActorComponent))) ActorError.NotComponentType(componentType);
        return ActorComponentType.GetId(componentType);
    }

    /// <summary>
    /// Получает тип компонента по его идентификатору.
    /// </summary>
    /// <param name="componentId">Идентификатор типа компонента.</param>
    /// <returns>Тип компонента.</returns>
    /// <exception cref="Exception">Выбрасывается, если по указанному идентификатору не найден тип компонента.</exception>
    public static Type GetComponentType(ushort componentId) => ActorComponentType.GetType(componentId);

    /// <summary>
    /// Получает ссылку на компонент по его индексу в пуле.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="context">Контекст актёра.</param>
    /// <param name="index">Индекс компонента в пуле.</param>
    /// <returns>Ссылка на компонент или нулевая ссылка, если компонент не найден.</returns>
    /// <exception cref="IndexOutOfRangeException">Выбрасывается, если индекс выходит за границы пула.</exception>
    public static ref T GetComponentByIndex<T>(ActorContext context, int index)
        where T : struct, IActorComponent
    {
        var pool = context.GetComponentPool<T>();
        if (pool == null) return ref Unsafe.NullRef<T>();

        return ref pool.GetByIndex(index);
    }

    /// <summary>
    /// Получает индекс компонента в пуле по идентификатору владельца.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="context">Контекст актёра.</param>
    /// <param name="ownerId">Идентификатор актёра-владельца.</param>
    /// <returns>Индекс компонента или -1, если компонент не найден.</returns>
    public static int GetComponentIndex<T>(ActorContext context, uint ownerId)
        where T : struct, IActorComponent
    {
        var pool = context.GetComponentPool<T>();
        return pool?.TryGetIndex(ownerId) ?? -1;
    }

    /// <summary>
    /// Получает владельца компонента.
    /// </summary>
    /// <typeparam name="T">Тип компонента.</typeparam>
    /// <param name="context">Контекст актёра.</param>
    /// <param name="component">Компонент, для которого ищется владелец.</param>
    /// <returns>Ссылка на актёра-владельца или пустая ссылка, если владелец не найден.</returns>
    /// <exception cref="Exception">Выбрасывается, если владелец компонента не найден.</exception>
    /// <remarks>
    /// Это медленный метод, так как он перебирает все компоненты указанного типа.
    /// </remarks>
    public static ActorRef<T> GetOwner<T>(ActorContext context, ref T component)
        where T : struct, IActorComponent
    {
        var pool = context.GetComponentPool<T>();
        if (pool != null)
        {
            foreach (var exists in pool)
            {
                if (Unsafe.AreSame(ref component, ref exists.Component1))
                {
                    return exists;
                }
            }
        }

        ActorError.ApplicableNotFound();
        return ActorRef<T>.Empty;
    }

    /// <summary>
    /// Реализует ли этот компонент интерфейс <see cref="IViewComponent"/>
    /// </summary>
    public static bool IsViewComponent<T>() where T : struct, IActorComponent => ActorComponentType<T>.IsView;

    /// <summary>
    /// Устанавливает ассет для актёра.
    /// </summary>
    /// <param name="actor">Актёр, которому назначается ассет.</param>
    /// <param name="asset">Ассет для установки.</param>
    public static void SetAsset(in Actor actor, in Asset asset)
    {
        actor.Context.SetBoundAsset(actor.Id, asset);
    }

    /// <summary>
    /// Удаляет ассет у актёра.
    /// </summary>
    /// <param name="actor">Актёр, у которого удаляется ассет.</param>
    public static void RemoveAsset(in Actor actor) => SetAsset(in actor, Asset.Empty);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetDebugContext([NotNullWhen(true)] out ActorContext? actorContext)
    {
        actorContext = WorldDebug.World?.Actors;
        return actorContext != null;
    }
}