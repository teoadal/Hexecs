using System.Text.Json;

namespace Hexecs.Actors.Components;

/// <summary>
/// Интерфейс для пула компонентов актёра, который управляет компонентами определенного типа
/// для множества актёров.
/// </summary>
internal interface IActorComponentPool
{
    /// <summary>
    /// Событие, вызываемое при добавлении компонента к актёру с указанным идентификатором.
    /// </summary>
    event Action<ActorId>? Added;
    
    /// <summary>
    /// Событие, вызываемое перед удалением компонента у актёра с указанным идентификатором.
    /// </summary>
    event Action<ActorId>? Removing;

    /// <summary>
    /// Контекст актёра, к которому принадлежит этот пул компонентов.
    /// </summary>
    ActorContext Context { get; }

    /// <summary>
    /// Количество компонентов, хранящихся в пуле.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Уникальный идентификатор типа компонента.
    /// </summary>
    ushort Id { get; }

    /// <summary>
    /// Тип компонентов, хранящихся в пуле.
    /// </summary>
    Type Type { get; }
    
    /// <summary>
    /// Очищает пул, удаляя все компоненты.
    /// </summary>
    void Clear();

    /// <summary>
    /// Клонирует компонент от одного актёра к другому.
    /// </summary>
    /// <param name="ownerId">Идентификатор актёра-источника.</param>
    /// <param name="cloneId">Идентификатор актёра-получателя.</param>
    void Clone(ActorId ownerId, ActorId cloneId);

    /// <summary>
    /// Получает компонент для актёра с указанным идентификатором.
    /// </summary>
    /// <param name="ownerId">Идентификатор актёра.</param>
    /// <returns>Компонент актёра.</returns>
    IActorComponent Get(ActorId ownerId);

    /// <summary>
    /// Проверяет, есть ли компонент у актёра с указанным идентификатором.
    /// </summary>
    /// <param name="ownerId">Идентификатор актёра.</param>
    /// <returns>Возвращает true, если компонент существует; иначе false.</returns>
    bool Has(ActorId ownerId);

    /// <summary>
    /// Удаляет компонент у актёра с указанным идентификатором.
    /// </summary>
    /// <param name="ownerId">Идентификатор актёра.</param>
    /// <returns>Возвращает true, если компонент был удален; иначе false.</returns>
    bool Remove(ActorId ownerId);

    /// <summary>
    /// Сериализует содержимое пула компонентов в формат JSON.
    /// </summary>
    /// <param name="writer">JSON-писатель для записи данных.</param>
    void Serialize(Utf8JsonWriter writer);
}