namespace Hexecs.Actors;

/// <summary>
/// Маркерный интерфейс для указания, что этот компонент является частью подсистемы отображения
/// </summary>
/// <remarks>
/// Используйте эти компоненты для обработки <see cref="IDrawSystem"/>.
/// </remarks>
public interface IViewComponent : IActorComponent;