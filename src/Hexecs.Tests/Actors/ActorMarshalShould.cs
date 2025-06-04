using System.Runtime.CompilerServices;
using Hexecs.Actors.Components;
using Hexecs.Tests.Mocks;

namespace Hexecs.Tests.Actors;

public sealed class ActorMarshalShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact]
    public void GetComponentId()
    {
        // arrange

        var expectedId = ActorComponentType<Attack>.Id;

        // act

        var actualId = ActorMarshal.GetComponentId<Attack>();

        // assert

        actualId
            .Should()
            .Be(expectedId);
    }

    [Fact]
    public void GetComponentType()
    {
        // arrange

        var id = ActorComponentType<Attack>.Id;

        // act

        var actualType = ActorMarshal.GetComponentType(id);

        // assert

        actualType
            .Should()
            .Be(typeof(Attack));
    }

    [Fact]
    public void GetComponentIndex()
    {
        // arrange

        var component = fixture.CreateComponent<Attack>();
        var actor = fixture.CreateActor<Attack>(component1: component);
        var expectedIndex = fixture.Actors
            .GetOrCreateComponentPool<Attack>()
            .TryGetIndex(actor.Id);

        // act

        var actualIndex = ActorMarshal.GetComponentIndex<Attack>(fixture.Actors, actor.Id);

        // assert

        actualIndex
            .Should()
            .BeGreaterThanOrEqualTo(0);

        actualIndex
            .Should()
            .Be(expectedIndex);
    }

    [Fact]
    public void GetComponentByIndex()
    {
        // arrange

        var component = fixture.CreateComponent<Attack>();
        var actor = fixture.CreateActor<Attack>(component1: component);
        var index = fixture.Actors
            .GetOrCreateComponentPool<Attack>()
            .TryGetIndex(actor.Id);

        // act

        ref var actualComponent = ref ActorMarshal.GetComponentByIndex<Attack>(fixture.Actors, index);

        // assert

        Unsafe
            .IsNullRef(ref actualComponent)
            .Should()
            .BeFalse();

        actualComponent
            .Should()
            .Be(component);
    }

    [Fact]
    public void GetComponentOwner()
    {
        // arrange

        var component = fixture.CreateComponent<Attack>();
        var actor = fixture.CreateActor<Attack>(component1: component);
        ref var componentRef = ref actor.Get<Attack>();

        // act

        var actualActor = ActorMarshal.GetOwner(fixture.Actors, ref componentRef);

        // assert

        actualActor.Id
            .Should()
            .Be(actor.Id);

        actualActor.Component1
            .Should()
            .Be(component);
    }
}