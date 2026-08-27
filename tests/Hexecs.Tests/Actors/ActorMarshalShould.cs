using Hexecs.Actors.Components;
using Hexecs.Tests.Mocks.ActorComponents;

namespace Hexecs.Tests.Actors;

public sealed class ActorMarshalShould(ActorTestFixture fixture) : IClassFixture<ActorTestFixture>
{
    [Fact]
    public void GetComponentId()
    {
        // arrange

        ushort expectedId = ActorComponentType<Attack>.Id;

        // act

        ushort actualId = ActorMarshal.GetComponentId<Attack>();

        // assert

        actualId
            .Should()
            .Be(expectedId);
    }

    [Fact]
    public void GetComponentType()
    {
        // arrange

        ushort id = ActorComponentType<Attack>.Id;

        // act

        Type actualType = ActorMarshal.GetComponentType(id);

        // assert

        actualType
            .Should()
            .Be(typeof(Attack));
    }

    [Fact]
    public void GetComponentOwner()
    {
        // arrange

        var component = fixture.CreateComponent<Attack>();
        Actor actor = fixture.CreateActor<Attack>(component1: component);
        ref Attack componentRef = ref actor.Get<Attack>();

        // act

        ActorRef<Attack> actualActor = ActorMarshal.GetOwner(fixture.Actors, ref componentRef);

        // assert

        actualActor.Id
            .Should()
            .Be(actor.Id);

        actualActor.Component1
            .Should()
            .Be(component);
    }
}
