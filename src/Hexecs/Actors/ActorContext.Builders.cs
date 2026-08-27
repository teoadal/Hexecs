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
        uint actorIdRaw = GetNextActorId();

        AddEntry(actorIdRaw);

        var actor = new Actor(this, new ActorId(actorIdRaw));

        if (asset.IsEmpty)
        {
            return actor;
        }

        args ??= Args.Rent();

        foreach (IActorBuilder builder in _builders)
        {
            builder.Build(in actor, in asset, args);
        }

        actor.Add(new BoundComponent(asset.Id));
        args.Return();

        return actor;
    }

    internal void LoadBuilders(IEnumerable<IActorBuilder> builders)
    {
        _builders = [.. builders];
    }
}
