using Hexecs.Assets;
using Hexecs.Assets.Sources;
using Hexecs.Tests.Mocks.ActorComponents;
using Hexecs.Tests.Mocks.Assets;
using Hexecs.Worlds;

namespace Hexecs.Tests.Actors;

public sealed class ActorTestFixture : BaseFixture, IDisposable
{
    public ActorContext Actors => World.Actors;
    public AssetContext Assets => World.Assets;
    public readonly World World;

    public ActorTestFixture()
    {
        World = new WorldBuilder()
            .CreateAssetData(CreateAssets)
            .UseDefaultActorContext(ctx => ctx
                .CreateBuilder<AttackBuilder>()
                .CreateBuilder<DefenceBuilder>()
                .ConfigureComponentPool<DisposableComponent>(c => c.AddDisposeHandler()))
            .Build();
    }

    public Actor CreateActor(uint? id = null)
    {
        var actor = Actors.CreateActor(id);
        return actor;
    }

    public Actor<T1> CreateActor<T1>(uint? id = null, T1? component1 = null)
        where T1 : struct, IActorComponent
    {
        var actor = Actors.CreateActor(id);
        actor.Add(component1 ?? CreateComponent<T1>());

        return actor.As<T1>();
    }

    public Actor<T1> CreateActor<T1, T2>(uint? id = null, T1? component1 = null, T2? component2 = null)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        var actor = Actors.CreateActor(id);
        actor.Add(component1 ?? CreateComponent<T1>());
        actor.Add(component2 ?? CreateComponent<T2>());

        return actor.As<T1>();
    }

    public Actor<T1> CreateActor<T1, T2, T3>(uint? id = null, T1? component1 = null, T2? component2 = null,
        T3? component3 = null)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        var actor = Actors.CreateActor(id);
        actor.Add(component1 ?? CreateComponent<T1>());
        actor.Add(component2 ?? CreateComponent<T2>());
        actor.Add(component3 ?? CreateComponent<T3>());

        return actor.As<T1>();
    }

    public Actor[] CreateActors<T1>(int? length = null)
        where T1 : struct, IActorComponent
    {
        length ??= RandomInt(10, 100);

        if (length is 0) return [];

        var actors = new Actor[length.Value];
        for (var i = 0; i < actors.Length; i++)
        {
            var actor = Actors.CreateActor();
            actor.Add(CreateComponent<T1>());
        }

        return actors;
    }

    public Actor[] CreateActors<T1, T2>(int? length = null)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
    {
        length ??= RandomInt(10, 100);

        if (length is 0) return [];

        var actors = new Actor[length.Value];
        for (var i = 0; i < actors.Length; i++)
        {
            var actor = Actors.CreateActor();
            actor.Add(CreateComponent<T1>());
            actor.Add(CreateComponent<T2>());

            actors[i] = actor;
        }

        return actors;
    }

    public Actor[] CreateActors<T1, T2, T3>(int? length = null)
        where T1 : struct, IActorComponent
        where T2 : struct, IActorComponent
        where T3 : struct, IActorComponent
    {
        length ??= RandomInt(10, 100);

        if (length is 0) return [];

        var actors = new Actor[length.Value];
        for (var i = 0; i < actors.Length; i++)
        {
            var actor = Actors.CreateActor();
            actor.Add(CreateComponent<T1>());
            actor.Add(CreateComponent<T2>());
            actor.Add(CreateComponent<T3>());

            actors[i] = actor;
        }

        return actors;
    }

    public T CreateComponent<T>() where T : struct, IActorComponent
    {
        object? result = null;

        if (typeof(T) == typeof(Attack)) result = new Attack { Value = RandomInt() };
        if (typeof(T) == typeof(Defence)) result = new Defence { Value = RandomInt() };
        if (typeof(T) == typeof(Description)) result = new Description { Name = RandomString(10) };
        if (typeof(T) == typeof(Speed)) result = new Speed { Value = RandomInt() };

        return result == null
            ? throw new NotSupportedException()
            : (T)result;
    }

    private void CreateAssets(IAssetLoader loader)
    {
        var unit1 = loader.CreateAsset(UnitAsset.Alias1);
        unit1.Set(new UnitAsset(RandomInt(1, 10), RandomInt(11, 20)));

        var unit2 = loader.CreateAsset(UnitAsset.Alias2);
        unit2.Set(new UnitAsset(RandomInt(1, 10), RandomInt(11, 20)));
    }

    public void Dispose()
    {
        World.Dispose();
    }
}