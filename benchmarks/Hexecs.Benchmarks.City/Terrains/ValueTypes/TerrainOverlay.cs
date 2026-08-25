namespace Hexecs.Benchmarks.Map.Terrains.ValueTypes;

public enum TerrainOverlay : byte
{
    None = 0,

    // --- Природные состояния (1-19) ---
    /// <summary>
    /// Снежный покров.
    /// </summary>
    Snow = 1,
    /// <summary>
    /// Тонкий слой льда (на воде или на дороге).
    /// </summary>
    Ice = 2,
    /// <summary>
    /// Лужи или затопление после дождя.
    /// </summary>
    Puddles = 3,

    // --- Растительность (20-39) ---
    /// <summary>
    /// Камыш или водные растения.
    /// </summary>
    Reeds = 20,
    /// <summary>
    /// Дикие кустарники или густая трава.
    /// </summary>
    Bushes = 21,
    /// <summary>
    /// Мох или лишайник (на камнях/бетоне).
    /// </summary>
    Moss = 22,
    /// <summary>
    /// Опавшие листья (городской декор).
    /// </summary>
    DeadLeaves = 23,

    // --- Городские и техногенные эффекты (40-59) ---
    /// <summary>
    /// Мусор или строительные обломки (например, после сноса).
    /// </summary>
    Debris = 40,
    /// <summary>
    /// Пятна масла, топлива или химикатов.
    /// </summary>
    PollutionSpill = 41,
    /// <summary>
    /// Следы износа или трещины на асфальте.
    /// </summary>
    Cracks = 42,
    
    // --- Следы событий (60-79) ---
    /// <summary>
    /// Следы гари после пожара.
    /// </summary>
    BurnMarks = 60,
    /// <summary>
    /// Кровь или следы происшествий.
    /// </summary>
    Blood = 61
}