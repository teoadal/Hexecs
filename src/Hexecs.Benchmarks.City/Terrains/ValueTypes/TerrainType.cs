namespace Hexecs.Benchmarks.Map.Terrains.ValueTypes;

// ReSharper disable ConvertToExtensionBlock
public enum TerrainType : byte
{
    None = 0,

    // Природная земля (1-19)
    Ground = 1, // пустая земля (например, сняли дёрн)
    GroundGrass = 2,
    GroundClay = 3,
    GroundSand = 4,
    GroundDirt = 5,

    // Подготовленная городская почва (20-39)
    UrbanGravel = 20, // Гравийная засыпка
    UrbanPavement = 21, // Мощение
    UrbanConcrete = 22, // Бетонное основание

    // Камни и минералы (40-59)
    Rock = 40,

    // Болота (60-79)
    Swamp = 60, // Глубокое болото (не проходимое)

    // Горы (вертикальные препятствия) (80-99)
    Mountains = 80,
    Cliff = 81, // Утёс, резкий перепад высоты

    // Вода (100-119)
    WaterShallow = 100,
    WaterRiver = 101, // (не проходимое)
    WaterOcean = 102, // (не проходимое)
}

public static class TerrainTypeExtensions
{
    public static bool IsGround(this TerrainType type)
    {
        return type is >= TerrainType.GroundGrass and < TerrainType.UrbanGravel;
    }

    public static bool IsUrban(this TerrainType type) => type is >= TerrainType.UrbanGravel and < TerrainType.Rock;

    public static bool IsRock(this TerrainType type) => type is >= TerrainType.Rock and < TerrainType.Swamp;

    public static bool IsSwamp(this TerrainType type) => type is >= TerrainType.Swamp and < TerrainType.Mountains;

    public static bool IsElevationObstacle(this TerrainType type)
    {
        return type is >= TerrainType.Mountains and < TerrainType.WaterShallow;
    }

    public static bool IsWater(this TerrainType type) => type >= TerrainType.WaterShallow;

    /// <summary>
    /// Проверка на проходимость для пеших юнитов.
    /// </summary>
    public static bool IsWalkable(this TerrainType type) => type switch
    {
        TerrainType.Swamp => false,
        TerrainType.Mountains => false,
        TerrainType.WaterRiver => false,
        TerrainType.WaterOcean => false,
        _ => true
    };
}