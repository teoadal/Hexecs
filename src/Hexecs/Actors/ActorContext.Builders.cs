using Hexecs.Actors.Bounds;
using Hexecs.Assets;

namespace Hexecs.Actors;

public sealed partial class ActorContext
{
    private IActorBuilder[] _builders;

    /// <summary>
    /// Создает актёра на основе указанного ассета.
    /// Построение осуществляется с помощью зарегистрированных <see cref="IActorBuilder"/>.
    /// </summary>
    /// <param name="asset">Ассет, используемый для построения актёра.</param>
    /// <param name="args">Дополнительные аргументы для построения актёра. Если не указаны, будут арендованы пустые аргументы.</param>
    /// <returns>Созданный актёр.</returns>
    /// <remarks>
    /// Создаёт для актёра специальный компонент, который позволяет получить <see cref="Asset"/>,
    /// по которому он был построен. Для получения ассета используйте метод <see cref="Actor.GetAsset"/>.
    /// </remarks>
    public Actor BuildActor(in Asset asset, Args? args = null)
    {
        var actorId = GetNextActorId();

        AddEntry(actorId);

        var actor = new Actor(this, actorId);

        if (asset.IsEmpty) return actor;
        
        args ??= Args.Rent();
        foreach (var builder in _builders)
        {
            builder.Build(in actor, in asset, args);
        }

        actor.Add(new BoundComponent(asset.Id));
        args.Return();

        return actor;
    }

    /// <summary>
    /// Создает типизированного актёра с компонентом указанного типа на основе ассета.
    /// </summary>
    /// <typeparam name="TComponent">Тип компонента, который будет иметь актёр.</typeparam>
    /// <param name="asset">Ассет, используемый для построения актёра.</param>
    /// <param name="args">Дополнительные аргументы для построения актёра. Если не указаны, будут арендованы пустые аргументы.</param>
    /// <returns>Типизированный актёр с компонентом указанного типа.</returns>
    /// <remarks>
    /// Метод создает обычного актёра с помощью <see cref="BuildActor"/>, а затем преобразует его в типизированного актёра.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Actor<TComponent> BuildActor<TComponent>(in Asset asset, Args? args = null)
        where TComponent : struct, IActorComponent
    {
        return BuildActor(in asset, args).As<TComponent>();
    }

    internal void LoadBuilders(IEnumerable<IActorBuilder> builders)
    {
        _builders = builders.ToArray();
    }
}