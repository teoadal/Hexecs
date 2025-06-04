using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Configurations;
using Hexecs.Dependencies;
using Hexecs.Loggers;
using Hexecs.Values;

namespace Hexecs.Worlds;

/// <summary>
/// Класс, представляющий собой основной контейнер игрового мира.
/// </summary>
/// <remarks>
/// Управляет контекстами актёров, ассетами и зависимостями.
/// Служит центральной точкой доступа к различным подсистемам.
/// </remarks>
public sealed class World : IDependencyProvider, IDisposable
{
    /// <summary>
    /// Контекст ассетов, предоставляющий доступ к управлению ресурсами в мире.
    /// </summary>
    public readonly AssetContext Assets;

    /// <summary>
    /// Контекст актёров, который указан как контекст по-умолчанию
    /// </summary>
    public readonly ActorContext Actors;

    /// <summary>
    /// Цикл обновления
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int Cycle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cycle;
    }

    /// <summary>
    /// Генератор случайных чисел, используемый в мире.
    /// </summary>
    public readonly Dice Dice;

    /// <summary>
    /// Состояние мира
    /// </summary>
    // ReSharper disable once ConvertToAutoProperty
    public WorldState State
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _state;
    }

    public readonly ValueService Values;

    private ActorContext?[] _actorContexts = new ActorContext?[4];
    private int _nextActorContextId = -1;

    private readonly DependencyProvider _services;

    private int _cycle;
    private long _previousDraw;
    private long _previousUpdate;
    private readonly long _startTime;
    private WorldState _state;

    internal World(
        ConfigurationService configurationService,
        LogService logService,
        DependencyProvider services,
        Action<ActorContextBuilder> defaultActorContextBuilder,
        ValueService valueService)
    {
        Assets = new AssetContext(this);
        Dice = new Dice();
        Values = valueService;

        _services = services;
        _services.Add(DependencyKey.First(typeof(AssetContext)), Assets);
        _services.Add(DependencyKey.First(typeof(ConfigurationService)), configurationService);
        _services.Add(DependencyKey.First(typeof(Dice)), Dice);
        _services.Add(DependencyKey.First(typeof(LogService)), logService);
        _services.Add(DependencyKey.First(typeof(ValueService)), valueService);
        _services.Add(DependencyKey.First(typeof(World)), this);

        var time = Environment.TickCount64;
        _previousDraw = time;
        _previousUpdate = time;
        _startTime = time;

        Actors = CreateActorContextImpl(true, defaultActorContextBuilder);
        _services.Add(DependencyKey.First(typeof(ActorContext)), Actors);

        _state = WorldState.None;
    }

    /// <summary>
    /// Создаёт новый контекст актёров в мире.
    /// </summary>
    /// <param name="context">Делегат для конфигурирования создаваемого контекста.</param>
    /// <returns>Созданный контекст актёров.</returns>
    public ActorContext CreateActorContext(Action<ActorContextBuilder> context)
    {
        return CreateActorContextImpl(false, context);
    }

    /// <summary>
    /// Освобождает ресурсы, используемые миром.
    /// Очищает все контексты актёров и освобождает зависимости.
    /// </summary>
    public void Dispose()
    {
        _services.Dispose();

        foreach (var context in _actorContexts)
        {
            context?.Clear();
        }

        ArrayUtils.Clear(_actorContexts);
    }

    public void Draw(TimeSpan elapsed, TimeSpan total) => Draw(new WorldTime(_cycle, elapsed, total));

    /// <summary>
    /// Рисует мир, в текущем цикле игрового времени.
    /// </summary>
    /// <param name="time">Параметры времени для отрисовки. Если null, будет создан новый экземпляр.</param>
    /// <remarks>
    /// Рисует все контексты актёров в мире с помощью <see cref="IDrawSystem"/>
    /// </remarks>
    /// <exception cref="Exception">Если уже запущен цикл обновления</exception>
    public void Draw(WorldTime? time = null)
    {
        var state = Interlocked.CompareExchange(ref _state, WorldState.Draw, WorldState.None);
        if (state != WorldState.None) WorldError.InvalidState(state);

        var now = Environment.TickCount64;

        var worldTime = time ?? new WorldTime(_cycle, now - _previousDraw, now - _startTime);

        foreach (var actorContext in _actorContexts)
        {
            actorContext?.Draw(worldTime);
        }

        _previousDraw = now;

        Interlocked.Exchange(ref _state, WorldState.None);
    }

    /// <summary>
    /// Получает контекст актёров по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор контекста актёров.</param>
    /// <returns>Контекст актёров с указанным идентификатором.</returns>
    /// <exception cref="Exception">Выбрасывается, если контекст с указанным идентификатором не найден.</exception>
    public ActorContext GetActorContext(int id)
    {
        var instance = id < _actorContexts.Length
            ? _actorContexts[id]
            : null;

        if (instance == null) WorldError.ActorContextNotFound(id);
        return instance;
    }

    /// <summary>
    /// Возвращает сервис указанного типа из провайдера зависимостей.
    /// </summary>
    /// <param name="contract">Тип запрашиваемого сервиса.</param>
    /// <returns>Экземпляр сервиса или null, если сервис не найден.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetService(Type contract) => _services.GetService(contract);

    /// <summary>
    /// Возвращает сервис указанного типа из провайдера зависимостей.
    /// </summary>
    /// <typeparam name="TService">Тип запрашиваемого сервиса.</typeparam>
    /// <returns>Экземпляр сервиса или null, если сервис не найден.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TService? GetService<TService>() where TService : class => _services.GetService<TService>();

    /// <summary>
    /// Возвращает все сервисы указанного типа из провайдера зависимостей.
    /// </summary>
    /// <typeparam name="TService">Тип запрашиваемых сервисов.</typeparam>
    /// <returns>Массив сервисов указанного типа.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TService[] GetServices<TService>() where TService : class => _services.GetServices<TService>();

    /// <summary>
    /// Удаляет указанный контекст актёров из мира.
    /// </summary>
    /// <param name="context">Контекст актёров для удаления.</param>
    /// <returns>Возвращает true, если контекст был успешно удален; иначе false.</returns>
    public bool RemoveActorContext(ActorContext context)
    {
        for (var i = 0; i < _actorContexts.Length; i++)
        {
            ref var exists = ref _actorContexts[i];
            if (exists == null || exists.Id != context.Id) continue;

            context.Clear();
            exists = null;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Пытается получить контекст актёров по указанному идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор контекста актёров.</param>
    /// <param name="context">Результирующий контекст актёров.</param>
    /// <returns>Возвращает true, если контекст был найден; иначе false.</returns>
    public bool TryGetActorContext(int id, out ActorContext context)
    {
        var instance = id < _actorContexts.Length
            ? _actorContexts[id]
            : null;

        context = instance!;
        return instance != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(TimeSpan elapsed, TimeSpan total) => Update(new WorldTime(_cycle, elapsed, total));

    /// <summary>
    /// Обновляет мир, выполняя один цикл игрового времени.
    /// </summary>
    /// <param name="time">Параметры времени для обновления. Если null, будет создан новый экземпляр.</param>
    /// <remarks>
    /// Обновляет все контексты актёров в мире с использованием <see cref="IUpdateSystem"/>
    /// </remarks>
    /// <exception cref="Exception">Если уже запущен цикл рисования</exception>
    public void Update(WorldTime? time = null)
    {
        var state = Interlocked.CompareExchange(ref _state, WorldState.Update, WorldState.None);
        if (state != WorldState.None) WorldError.InvalidState(state);

        var now = Environment.TickCount64;
        _cycle++;

        var worldTime = time ?? new WorldTime(_cycle, now - _previousUpdate, now - _startTime);

        foreach (var actorContext in _actorContexts)
        {
            actorContext?.Update(worldTime);
        }

        _previousUpdate = now;

        Interlocked.Exchange(ref _state, WorldState.None);
    }

    private ActorContext CreateActorContextImpl(bool isDefault, Action<ActorContextBuilder> context)
    {
        var id = Interlocked.Increment(ref _nextActorContextId);

        var builder = new ActorContextBuilder(
            isDefault,
            id,
            _services.GetScopeProvider(),
            this);

        context(builder);

        var instance = builder.Build();
        ArrayUtils.EnsureCapacity(ref _actorContexts, id);
        _actorContexts[id] = instance;

        return instance;
    }
}