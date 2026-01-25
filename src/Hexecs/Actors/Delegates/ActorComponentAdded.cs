namespace Hexecs.Actors.Delegates;

/// <summary>
/// Делегат, вызываемый при добавлении компонента к актёру.
/// </summary>
/// <typeparam name="T">Тип компонента, должен быть структурой и реализовывать интерфейс IActorComponent.</typeparam>
/// <param name="actorId">Идентификатор актёра, к которому был добавлен компонент.</param>
/// <param name="component">Ссылка на добавленный компонент.</param>
public delegate void ActorComponentAdded<T>(uint actorId, ref T component)
    where T : struct, IActorComponent;
