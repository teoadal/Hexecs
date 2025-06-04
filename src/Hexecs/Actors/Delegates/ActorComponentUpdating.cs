namespace Hexecs.Actors.Delegates;

/// <summary>
/// Делегат, который вызывается при обновлении компонента актёра.
/// </summary>
/// <typeparam name="T">Тип компонента актёра.</typeparam>
/// <param name="actorId">Идентификатор актёра, чей компонент обновляется.</param>
/// <param name="exists">Текущее состояние компонента актёра.</param>
/// <param name="expected">Ожидаемое (новое) состояние компонента актёра.</param>
public delegate void ActorComponentUpdating<T>(uint actorId, ref T exists, in T expected)
    where T : struct, IActorComponent;