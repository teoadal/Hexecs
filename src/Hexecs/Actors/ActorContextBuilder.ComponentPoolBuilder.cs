using Hexecs.Actors.Components;
using Hexecs.Actors.Delegates;
using Hexecs.Actors.Serializations;

namespace Hexecs.Actors;

public sealed class ActorComponentPoolBuilder<TComponent>
    where TComponent : struct, IActorComponent
{
    private int? _capacity;
    private ActorCloneHandler<TComponent>? _cloneHandler;
    private IActorComponentConverter<TComponent>? _converter;
    private ActorDisposeHandler<TComponent>? _disposeHandler;

    internal ActorComponentPoolBuilder()
    {
    }

    internal ActorComponentConfiguration<TComponent> Build() => new(_capacity, _cloneHandler, _disposeHandler, _converter);

    public ActorComponentPoolBuilder<TComponent> AddCloneHandler(ActorCloneHandler<TComponent> handler)
    {
        _cloneHandler = handler;
        return this;
    }

    public ActorComponentPoolBuilder<TComponent> AddConverter(IActorComponentConverter<TComponent> converter)
    {
        _converter = converter;
        return this;
    }

    public ActorComponentPoolBuilder<TComponent> AddDisposeHandler(ActorDisposeHandler<TComponent> handler)
    {
        _disposeHandler = handler;
        return this;
    }

    public ActorComponentPoolBuilder<TComponent> Capacity(int capacity)
    {
        _capacity = capacity;
        return this;
    }

    public ActorComponentPoolBuilder<TComponent> CreateConverter<TConverter>()
        where TConverter : class, IActorComponentConverter<TComponent>, new()
    {
        _converter = new TConverter();
        return this;
    }
}

public static class ActorComponentPoolBuilderExtensions
{
    /// <summary>
    /// Добавить обработчик клонирования компонентов, реализующих интерфейс <see cref="ICloneable{T}"/> 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActorComponentPoolBuilder<TComponent> AddCloneHandler<TComponent>(
        this ActorComponentPoolBuilder<TComponent> builder)
        where TComponent : struct, IActorComponent, ICloneable<TComponent>
    {
        return builder.AddCloneHandler(static (in TComponent component) => component.Clone());
    }

    /// <summary>
    /// Добавить обработчик очистки для компонентов, реализующих интерфейс <see cref="IDisposable"/> 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActorComponentPoolBuilder<TComponent> AddDisposeHandler<TComponent>(
        this ActorComponentPoolBuilder<TComponent> builder)
        where TComponent : struct, IActorComponent, IDisposable
    {
        return builder.AddDisposeHandler(static (ref TComponent component) => component.Dispose());
    }
}