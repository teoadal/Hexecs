namespace Hexecs.Assets;

/// <summary>
/// Статический класс, предоставляющий методы для генерации исключений, связанных с ошибками работы с ассетами.
/// Используется для централизованной обработки ошибок в системе управления ассетами.
/// </summary>
internal static class AssetError
{
    /// <summary>
    /// Генерирует исключение при попытке найти псевдоним (алиас) ассета, который не существует.
    /// </summary>
    /// <param name="assetId">Идентификатор ассета, для которого не найден псевдоним.</param>
    [DoesNotReturn]
    public static void AliasNotFound(uint assetId)
    {
        throw new Exception($"Alias for asset {assetId} isn't found");
    }

    /// <summary>
    /// Генерирует исключение при попытке создать ассет с идентификатором, который уже используется.
    /// </summary>
    /// <param name="assetId">Идентификатор уже существующего ассета.</param>
    [DoesNotReturn]
    public static void AlreadyExists(uint assetId)
    {
        throw new Exception($"Asset with id {assetId} already exists");
    }

    /// <summary>
    /// Генерирует исключение, когда подходящий ассет не найден.
    /// </summary>
    [DoesNotReturn]
    public static void ApplicableNotFound()
    {
        throw new Exception("Applicable asset isn't found");
    }
    
    /// <summary>
    /// Генерирует исключение, когда подходящий ассет с указанным компонентом не найден.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который должен содержать ассет.</typeparam>
    [DoesNotReturn]
    public static void ApplicableNotFound<T>()
    {
        throw new Exception($"Applicable asset with component {TypeOf<T>.GetTypeName()} isn't found");
    }
    
    /// <summary>
    /// Генерирует исключение, когда не найден тип компонента ассета по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор типа компонента.</param>
    [DoesNotReturn]
    public static void ComponentTypeNotFound(uint id)
    {
        throw new Exception($"Asset component type with id '{id}' isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда ассет с указанным псевдонимом не найден.
    /// </summary>
    /// <param name="alias">Псевдоним (алиас) ассета.</param>
    [DoesNotReturn]
    public static void NotFound(string alias)
    {
        throw new Exception($"Asset with alias '{alias}' isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда ассет уже содержит указанный компонент.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который уже существует в ассете.</typeparam>
    /// <param name="assetId">Идентификатор ассета.</param>
    [DoesNotReturn]
    public static void ComponentAlreadyExists<T>(uint assetId)
    {
        throw new Exception($"Asset {assetId} already has component {TypeOf<T>.GetTypeName()}");
    }

    /// <summary>
    /// Генерирует исключение, когда компонент указанного типа не найден в ассете.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который отсутствует в ассете.</typeparam>
    /// <param name="assetId">Идентификатор ассета.</param>
    [DoesNotReturn]
    public static void ComponentNotFound<T>(uint assetId)
    {
        throw new Exception($"Asset {assetId} don't have component {TypeOf<T>.GetTypeName()}");
    }

    /// <summary>
    /// Генерирует исключение, когда ограничение для указанного типа уже существует.
    /// </summary>
    /// <typeparam name="T">Тип, для которого ограничение уже определено.</typeparam>
    [DoesNotReturn]
    public static void ConstraintExists<T>()
    {
        throw new Exception($"Constraint for {TypeOf<T>.GetTypeName()} already exists");
    }

    [DoesNotReturn]
    public static void InvalidId()
    {
        throw new Exception("Invalid asset id");
    }
    
    /// <summary>
    /// Генерирует исключение при попытке использовать загрузчик ассетов, который уже освобожден.
    /// </summary>
    [DoesNotReturn]
    public static void LoaderDisposed()
    {
        throw new Exception("Loader is disposed");
    }

    /// <summary>
    /// Генерирует исключение, когда ассет с указанным компонентом не найден.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который должен содержать ассет.</typeparam>
    [DoesNotReturn]
    public static void NotFound<T>()
    {
        throw new Exception($"Asset with component {TypeOf<T>.GetTypeName()} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда ассет с указанным идентификатором не найден.
    /// </summary>
    /// <param name="assetId">Идентификатор ассета, который не найден.</param>
    [DoesNotReturn]
    public static void NotFound(uint assetId)
    {
        throw new Exception($"Asset with id {assetId} isn't found");
    }
}