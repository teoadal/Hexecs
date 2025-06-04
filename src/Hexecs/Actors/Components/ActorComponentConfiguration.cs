using Hexecs.Actors.Delegates;
using Hexecs.Actors.Serializations;

namespace Hexecs.Actors.Components;

/// <summary>
/// Конфигурация пула компонентов актёра определенного типа.
/// </summary>
/// <typeparam name="T">Тип компонента актёра</typeparam>
/// <param name="capacity">Начальная емкость пула компонентов</param>
/// <param name="cloneHandler">Обработчик для клонирования компонентов</param>
/// <param name="disposeHandler">Обработчик для освобождения ресурсов компонентов</param>
/// <param name="converter">Конвертер для сериализации/десериализации компонентов</param>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
internal sealed class ActorComponentConfiguration<T>(
    int? capacity,
    ActorCloneHandler<T>? cloneHandler,
    ActorDisposeHandler<T>? disposeHandler,
    IActorComponentConverter<T>? converter) : IActorComponentConfiguration
    where T : struct, IActorComponent
{
    /// <summary>
    /// Пустая конфигурация без каких-либо настроек
    /// </summary>
    public static readonly ActorComponentConfiguration<T> Empty = new(
        null,
        null,
        null,
        null);

    /// <summary>
    /// Начальная емкость пула компонентов. Null означает использование емкости по умолчанию.
    /// </summary>
    public readonly int? Capacity = capacity;

    /// <summary>
    /// Обработчик для создания копий компонентов. Используется при клонировании актёров.
    /// </summary>
    public readonly ActorCloneHandler<T>? CloneHandler = cloneHandler;

    /// <summary>
    /// Обработчик для освобождения ресурсов компонентов при их удалении из пула.
    /// </summary>
    public readonly ActorDisposeHandler<T>? DisposeHandler = disposeHandler;

    /// <summary>
    /// Конвертер для сериализации и десериализации компонентов.
    /// </summary>
    public readonly IActorComponentConverter<T>? Converter = converter;
}

/// <summary>
/// Маркерный интерфейс для конфигурации компонентов актёра
/// </summary>
internal interface IActorComponentConfiguration;