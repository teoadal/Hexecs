namespace Hexecs.Actors.Delegates;

/// <summary>
/// Делегат, вызываемый при удалении компонента актёра.
/// </summary>
/// <typeparam name="T">Тип компонента, должен быть структурой и реализовывать интерфейс <see cref="IActorComponent"/>.</typeparam>
/// <param name="actorId">Идентификатор актёра, к которому был добавлен компонент.</param>
/// <param name="component">Удалённый компонент.</param>
public delegate void ActorComponentRemoving<T>(uint actorId, ref T component)
    where T : struct, IActorComponent;