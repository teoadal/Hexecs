using Hexecs.Benchmarks.Map.Terrains.ValueTypes;
using Hexecs.Benchmarks.Map.ValueTypes;

namespace Hexecs.Benchmarks.Map.Terrains;

public struct Terrain : IActorComponent
{
    /// <summary>
    /// Высота (100 - уровень моря, 150 - холм, 250 - гора)
    /// </summary>
    public Elevation Elevation;

    /// <summary>
    /// Влажность или загрязнение (100 - это 0)
    /// </summary>
    public Moisture Moisture;

    /// <summary>
    /// Покрытие
    /// </summary>
    public TerrainOverlay Overlay;

    /// <summary>
    /// Температура (100 - это 0)
    /// </summary>
    public Temperature Temperature;

    /// <summary>
    /// Основной тип
    /// </summary>
    public TerrainType Type;
}