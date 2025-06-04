using Hexecs.Actors.Bounds;
using Hexecs.Actors.Components;
using Hexecs.Actors.Nodes;
using Hexecs.Actors.Relations;
using Hexecs.Dependencies;
using Hexecs.Pipelines;
using Hexecs.Pipelines.Commands;
using Hexecs.Pipelines.Messages;
using Hexecs.Pipelines.Notifications;
using Hexecs.Pipelines.Queries;
using Hexecs.Worlds;

namespace Hexecs.Actors;

/// <summary>
/// Строитель контекста актёров, предоставляющий интерфейс для конфигурации контекста актёров в системе.
/// Позволяет регистрировать билдеры, обработчики команд, запросов, сообщений, уведомлений и системы актёров.
/// </summary>
public sealed partial class ActorContextBuilder
{
    /// <summary>
    /// Вместимость по умолчанию для контекста актёров.
    /// </summary>
    public const int DefaultCapacity = 256;

    /// <summary>
    /// Уникальный идентификатор контекста актёров.
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// Мир, в котором находится контекст актёров.
    /// </summary>
    public readonly World World;

    private readonly List<Entry<IActorBuilder>> _builders;
    private readonly List<Entry<ICommandHandler>> _commands;
    private readonly List<Entry<IQueryHandler>> _queries;
    private readonly List<Func<ActorContext, IMessageQueue>?> _messages;
    private readonly List<Entry<INotificationHandler>> _notifications;

    private readonly List<Entry<IDrawSystem>> _drawSystems;
    private readonly List<Entry<IUpdateSystem>> _updateSystems;

    private readonly List<IActorComponentConfiguration?> _componentConfigurations;

    private readonly DependencyProvider _dependencyProvider;

    private int _capacity;
    private readonly bool _isDefault;

    internal ActorContextBuilder(bool isDefault, int id, DependencyProvider dependencyProvider, World world)
    {
        Id = id;
        World = world;

        _builders = [];
        _commands = [];
        _queries = [];
        _messages = [];
        _notifications = [];

        _drawSystems = [];
        _updateSystems = [];

        _componentConfigurations = [];
        _componentConfigurations.Insert(
            ActorComponentType<BoundComponent>.Id,
            BoundComponent.CreatePoolConfiguration());
        _componentConfigurations.Insert(
            ActorComponentType<ActorNodeComponent>.Id,
            ActorNodeComponent.CreatePoolConfiguration());
        _componentConfigurations.Insert(
            ActorComponentType<ActorRelationComponent>.Id,
            ActorRelationComponent.CreatePoolConfiguration());

        _dependencyProvider = dependencyProvider;

        _capacity = DefaultCapacity;
        _isDefault = isDefault;
    }

    #region Builder

    /// <summary>
    /// Регистрирует готовый строитель актёров.
    /// </summary>
    /// <param name="builder">Экземпляр строителя актёров.</param>
    /// <returns>Этот же экземпляр ActorContextBuilder для цепочки вызовов.</returns>
    public ActorContextBuilder AddBuilder(IActorBuilder builder)
    {
        _builders.Add(new Entry<IActorBuilder>(builder));
        return this;
    }

    /// <summary>
    /// Регистрирует строитель актёров указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип строителя актёров.</typeparam>
    /// <returns>Этот же экземпляр ActorContextBuilder для цепочки вызовов.</returns>
    public ActorContextBuilder AddBuilder<T>() where T : class, IActorBuilder, new()
    {
        _builders.Add(new Entry<IActorBuilder>(static _ => new T()));
        return this;
    }

    /// <summary>
    /// Регистрирует функцию для создания строителя актёров с доступом к контексту актёров.
    /// </summary>
    /// <param name="builder">Функция, создающая строитель актёров.</param>
    /// <returns>Этот же экземпляр ActorContextBuilder для цепочки вызовов.</returns>
    public ActorContextBuilder CreateBuilder(Func<ActorContext, IActorBuilder> builder)
    {
        _builders.Add(new Entry<IActorBuilder>(builder));
        return this;
    }

    #endregion

    /// <summary>
    /// Предполагаемое количество сущностей контекста (по-умолчанию это <see cref="DefaultCapacity"/>)
    /// </summary>
    public ActorContextBuilder Capacity(int capacity)
    {
        _capacity = capacity;
        return this;
    }

    /// <summary>
    /// Настройка пула компонентов
    /// </summary>
    public ActorContextBuilder ConfigureComponentPool<TComponent>(
        Action<ActorComponentPoolBuilder<TComponent>> component)
        where TComponent : struct, IActorComponent
    {
        var index = (int)ActorComponentType<TComponent>.Id;

        CollectionsMarshal.SetCount(_componentConfigurations, index + 1);
        var span = CollectionsMarshal.AsSpan(_componentConfigurations);
        ref var value = ref span[index];

        if (value == null)
        {
            var builder = new ActorComponentPoolBuilder<TComponent>();
            component(builder);

            value = builder.Build();
        }
        else
        {
            ActorError.ComponentPoolAlreadyConfigured(typeof(TComponent));
        }


        return this;
    }

    #region Command

    /// <summary>
    /// Регистрирует обработчик команды указанного типа.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды.</typeparam>
    /// <param name="handler">Экземпляр обработчика команды.</param>
    public ActorContextBuilder AddCommandHandler<TCommand>(ICommandHandler<TCommand> handler)
        where TCommand : struct, ICommand<Result>
    {
        InsertCommandHandlerEntry(
            CommandType<TCommand>.Id,
            typeof(TCommand),
            new Entry<ICommandHandler>(handler));

        return this;
    }

    /// <summary>
    /// Регистрирует метод создания обработчика команды указанного типа с указанным типом результата.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды.</typeparam>
    /// <typeparam name="TResult">Тип результата команды.</typeparam>
    /// <param name="handler">Экземпляр обработчика команды.</param>
    public ActorContextBuilder AddCommandHandler<TCommand, TResult>(ICommandHandler<TCommand, TResult> handler)
        where TCommand : struct, ICommand<TResult>
    {
        InsertCommandHandlerEntry(
            CommandType<TCommand>.Id,
            typeof(TCommand),
            new Entry<ICommandHandler>(handler));

        return this;
    }

    /// <summary>
    /// Регистрирует функцию для создания обработчика команды указанного типа с доступом к контексту актёров.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды.</typeparam>
    /// <param name="handler">Функция, создающая обработчик команды.</param>
    public ActorContextBuilder CreateCommandHandler<TCommand>(Func<ActorContext, ICommandHandler<TCommand>> handler)
        where TCommand : struct, ICommand<Result>
    {
        InsertCommandHandlerEntry(
            CommandType<TCommand>.Id,
            typeof(TCommand),
            new Entry<ICommandHandler>(handler));

        return this;
    }

    /// <summary>
    /// Регистрирует функцию для создания обработчика команды указанного типа с указанным типом результата и доступом к контексту актёров.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды.</typeparam>
    /// <typeparam name="TResult">Тип результата команды.</typeparam>
    /// <param name="handler">Функция, создающая обработчик команды.</param>
    public ActorContextBuilder CreateCommandHandler<TCommand, TResult>(
        Func<ActorContext, ICommandHandler<TCommand, TResult>> handler)
        where TCommand : struct, ICommand<TResult>
    {
        InsertCommandHandlerEntry(
            CommandType<TCommand>.Id,
            typeof(TCommand),
            new Entry<ICommandHandler>(handler));

        return this;
    }

    internal void InsertCommandHandlerEntry(uint commandTypeId, Type commandType, Entry<ICommandHandler> entry)
    {
        var index = (int)commandTypeId;

        CollectionsMarshal.SetCount(_commands, index + 1);
        var span = CollectionsMarshal.AsSpan(_commands);
        ref var value = ref span[index];

        if (value.IsEmpty) value = entry;
        else PipelineError.CommandHandlerAlreadyRegistered(commandType);
    }

    #endregion

    #region Draw system

    /// <summary>
    /// Регистрирует систему отрисовки актёров.
    /// </summary>
    /// <param name="system">Экземпляр системы актёров.</param>
    public ActorContextBuilder AddDrawSystem(IDrawSystem system) => InsertDrawSystem(new Entry<IDrawSystem>(system));

    /// <summary>
    /// Регистрирует функцию для создания системы отрисовки актёров с доступом к контексту актёров.
    /// </summary>
    /// <param name="system">Функция, создающая систему актёров.</param>
    public ActorContextBuilder CreateDrawSystem(Func<ActorContext, IDrawSystem> system)
    {
        return InsertDrawSystem(new Entry<IDrawSystem>(system));
    }

    private ActorContextBuilder InsertDrawSystem(Entry<IDrawSystem> entry)
    {
        _drawSystems.Add(entry);
        return this;
    }

    #endregion

    #region Query

    /// <summary>
    /// Регистрирует обработчик запроса указанного типа с указанным типом результата.
    /// </summary>
    /// <typeparam name="TQuery">Тип запроса.</typeparam>
    /// <typeparam name="TResult">Тип результата запроса.</typeparam>
    /// <param name="handler">Экземпляр обработчика запроса.</param>
    public ActorContextBuilder AddQueryHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler)
        where TQuery : struct, IQuery<TResult>
    {
        InsertQueryHandlerEntry<TQuery, TResult>(new Entry<IQueryHandler>(handler));
        return this;
    }

    /// <summary>
    /// Регистрирует функцию для создания обработчика запроса указанного типа с указанным типом результата и доступом к контексту актёров.
    /// </summary>
    /// <typeparam name="TQuery">Тип запроса.</typeparam>
    /// <typeparam name="TResult">Тип результата запроса.</typeparam>
    /// <param name="handler">Функция, создающая обработчик запроса.</param>
    public ActorContextBuilder CreateQueryHandler<TQuery, TResult>(
        Func<ActorContext, IQueryHandler<TQuery, TResult>> handler)
        where TQuery : struct, IQuery<TResult>
    {
        InsertQueryHandlerEntry<TQuery, TResult>(new Entry<IQueryHandler>(handler));
        return this;
    }

    private void InsertQueryHandlerEntry<TQuery, TResult>(Entry<IQueryHandler> entry)
        where TQuery : struct, IQuery<TResult>
    {
        var index = (int)QueryType<TQuery>.Id;

        CollectionsMarshal.SetCount(_queries, index + 1);
        var span = CollectionsMarshal.AsSpan(_queries);
        ref var value = ref span[index];

        if (value.IsEmpty) value = entry;
        else PipelineError.QueryHandlerAlreadyRegistered(typeof(TQuery));
    }

    #endregion

    #region Message

    /// <summary>
    /// Регистрирует обработчик сообщения указанного типа.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="handler">Экземпляр обработчика сообщения.</param>
    public ActorContextBuilder AddMessageHandler<TMessage>(IMessageHandler<TMessage> handler)
        where TMessage : struct, IMessage => CreateMessageHandler(_ => handler);

    /// <summary>
    /// Регистрирует функцию для создания обработчика сообщения указанного типа с доступом к контексту актёров.
    /// </summary>
    /// <typeparam name="TMessage">Тип сообщения.</typeparam>
    /// <param name="handler">Функция, создающая обработчик сообщения.</param>
    public ActorContextBuilder CreateMessageHandler<TMessage>(Func<ActorContext, IMessageHandler<TMessage>> handler)
        where TMessage : struct, IMessage
    {
        var index = (int)MessageType<TMessage>.Id;

        CollectionsMarshal.SetCount(_messages, index + 1);
        var span = CollectionsMarshal.AsSpan(_messages);
        ref var value = ref span[index];

        if (value != null) PipelineError.MessageQueueAlreadyRegistered(typeof(TMessage));
        else value = context => new MessageQueue<TMessage>(handler(context));

        return this;
    }

    #endregion

    #region Notification

    /// <summary>
    /// Регистрирует обработчик уведомления указанного типа.
    /// </summary>
    /// <typeparam name="TNotification">Тип уведомления.</typeparam>
    /// <param name="handler">Экземпляр обработчика уведомления.</param>
    /// <returns>Этот же экземпляр ActorContextBuilder для цепочки вызовов.</returns>
    public ActorContextBuilder AddNotificationHandler<TNotification>(INotificationHandler<TNotification> handler)
        where TNotification : struct, INotification
    {
        var index = (int)NotificationType<TNotification>.Id;

        CollectionsMarshal.SetCount(_notifications, index + 1);
        var span = CollectionsMarshal.AsSpan(_notifications);
        ref var value = ref span[index];

        if (value.IsEmpty)
        {
            value = new Entry<INotificationHandler>(new SimpleNotificationPipeline<TNotification>(handler));
        }
        else
        {
            PipelineError.NotificationPipelineAlreadyRegistered(typeof(TNotification));
        }

        return this;
    }

    /// <summary>
    /// Регистрирует конвейер обработчиков уведомлений указанного типа.
    /// </summary>
    /// <typeparam name="TNotification">Тип уведомления.</typeparam>
    /// <param name="pipeline">Действие для конфигурации конвейера обработчиков уведомлений.</param>
    /// <returns>Этот же экземпляр ActorContextBuilder для цепочки вызовов.</returns>
    public ActorContextBuilder CreateNotificationHandler<TNotification>(
        Action<NotificationBuilder<TNotification>> pipeline)
        where TNotification : struct, INotification
    {
        var index = (int)NotificationType<TNotification>.Id;

        CollectionsMarshal.SetCount(_notifications, index + 1);
        var span = CollectionsMarshal.AsSpan(_notifications);
        ref var value = ref span[index];

        if (value.IsEmpty)
        {
            var builder = new NotificationBuilder<TNotification>();
            pipeline(builder);
            value = new Entry<INotificationHandler>(builder.Build());
        }
        else
        {
            PipelineError.NotificationPipelineAlreadyRegistered(typeof(TNotification));
        }

        return this;
    }

    #endregion

    #region Update system

    /// <summary>
    /// Регистрирует систему обновления актёров.
    /// </summary>
    /// <param name="system">Экземпляр системы актёров.</param>
    public ActorContextBuilder AddUpdateSystem(IUpdateSystem system)
    {
        return InsertUpdateSystem(new Entry<IUpdateSystem>(system));
    }

    /// <summary>
    /// Регистрирует функцию для создания системы обновления актёров с доступом к контексту актёров.
    /// </summary>
    /// <param name="system">Функция, создающая систему актёров.</param>
    public ActorContextBuilder CreateUpdateSystem(Func<ActorContext, IUpdateSystem> system)
    {
        return InsertUpdateSystem(new Entry<IUpdateSystem>(system));
    }

    /// <summary>
    /// Регистрирует систему параллельного обновления актёров с указанным порядком выполнения.
    /// </summary>
    /// <param name="order">Порядок выполнения системы.</param>
    /// <param name="parallelSystem">Действие для конфигурации параллельной системы.</param>
    public ActorContextBuilder CreateParallelUpdateSystem(int order, Action<ParallelSystemBuilder> parallelSystem)
    {
        var builder = new ParallelSystemBuilder(order);
        parallelSystem(builder);

        _updateSystems.Add(new Entry<IUpdateSystem>(builder.Build()));
        return this;
    }

    /// <summary>
    /// Регистрирует систему параллельного обновления актёров с максимальным порядком выполнения.
    /// </summary>
    /// <param name="parallelSystem">Действие для конфигурации параллельной системы.</param>
    public ActorContextBuilder CreateParallelUpdateSystem(Action<ParallelSystemBuilder> parallelSystem)
    {
        return CreateParallelUpdateSystem(int.MaxValue, parallelSystem);
    }

    private ActorContextBuilder InsertUpdateSystem(Entry<IUpdateSystem> entry)
    {
        _updateSystems.Add(entry);
        return this;
    }

    #endregion

    /// <summary>
    /// Создает контекст актёров на основе настроенной конфигурации.
    /// </summary>
    /// <returns>Новый экземпляр контекста актёров.</returns>
    internal ActorContext Build()
    {
        var instance = new ActorContext(
            _isDefault,
            Id,
            _dependencyProvider,
            World,
            _capacity,
            _componentConfigurations.ToArray());

        _componentConfigurations.Clear();

        // 1. Мы создаём шину данных в первую очередь.
        instance.LoadPipelines(
            _commands.ToArray(static (entry, ctx) => entry.IsEmpty ? null : entry.Invoke(ctx), instance),
            _queries.ToArray(static (entry, ctx) => entry.IsEmpty ? null : entry.Invoke(ctx), instance),
            _notifications.ToArray(static (entry, ctx) => entry.IsEmpty ? null : entry.Invoke(ctx), instance),
            _messages.ToArray(static (func, ctx) => func?.Invoke(ctx), instance));

        _commands.Clear();
        _queries.Clear();
        _notifications.Clear();
        _messages.Clear();

        // 2. Builder'ы вряд ли зависят от систем, но скорее всего будут зависеть от шины данных.
        instance.LoadBuilders(_builders
            .Select(static (entry, ctx) => entry.Invoke(ctx), instance)
            .Concat(World.GetServices<IActorBuilder>())
            .Order(OrderComparer<IActorBuilder>.CreateInstance()));

        _builders.Clear();

        // 3. Системы точно будут использовать шину и, возможно, builder'ы.
        instance.LoadSystems(
            _drawSystems
                .Select((entry, ctx) => entry.Invoke(ctx), instance)
                .Concat(World.GetServices<IDrawSystem>())
                .Order(OrderComparer<IDrawSystem>.CreateInstance()),
            _updateSystems
                .Select((entry, ctx) => entry.Invoke(ctx), instance)
                .Concat(World.GetServices<IUpdateSystem>())
                .Order(OrderComparer<IUpdateSystem>.CreateInstance()));

        _updateSystems.Clear();

        return instance;
    }
}