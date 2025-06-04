namespace Hexecs.Dependencies;

/// <summary>
/// Представляет зависимость с контрактом и временем жизни.
/// </summary>
internal readonly struct Dependency
{
    /// <summary>
    /// Получает тип контракта для этой зависимости.
    /// </summary>
    public readonly Type Contract;

    public readonly object? Instance;

    /// <summary>
    /// Получает время жизни зависимости.
    /// </summary>
    public readonly DependencyLifetime Lifetime;

    public readonly Func<IDependencyProvider, object>? Resolver;

    /// <summary>
    /// Инициализирует новый экземпляр структуры <see cref="Dependency"/> с использованием экземпляра.
    /// </summary>
    /// <param name="lifetime">Время жизни зависимости.</param>
    /// <param name="contract">Тип контракта для этой зависимости.</param>
    /// <param name="instance">Экземпляр, который будет использоваться в качестве зависимости.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dependency(DependencyLifetime lifetime, Type contract, object instance)
    {
        Contract = contract;
        Lifetime = lifetime;

        Instance = instance;
        Resolver = null;
    }

    /// <summary>
    /// Инициализирует новый экземпляр структуры <see cref="Dependency"/> с использованием резолвера.
    /// </summary>
    /// <param name="lifetime">Время жизни зависимости.</param>
    /// <param name="contract">Тип контракта для этой зависимости.</param>
    /// <param name="resolver">Функция для разрешения экземпляра зависимости.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Dependency(DependencyLifetime lifetime, Type contract, Func<IDependencyProvider, object> resolver)
    {
        Contract = contract;
        Lifetime = lifetime;

        Instance = null;
        Resolver = resolver;
    }

    /// <summary>
    /// Разрешает экземпляр зависимости с использованием предоставленного поставщика.
    /// </summary>
    /// <param name="provider">Поставщик зависимости.</param>
    /// <returns>Разрешенный объект для этой зависимости.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object Resolve(IDependencyProvider provider)
    {
        if (Instance != null) return Instance;
        if (Resolver == null) DependencyError.ResolverNotFound(Contract, Lifetime);
        return Resolver(provider);
    }
}