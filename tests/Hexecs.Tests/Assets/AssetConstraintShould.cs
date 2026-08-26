using Hexecs.Assets;
using Hexecs.Tests.Mocks.Assets;

namespace Hexecs.Tests.Assets;

public sealed class AssetConstraintShould(AssetTestFixture fixture) : IClassFixture<AssetTestFixture>
{
    [Fact(DisplayName = "Должен успешно проходить проверку Applicable, если все условия соблюдены")]
    public void Should_Be_Applicable_When_Conditions_Met()
    {
        // Arrange

        Asset asset = fixture.CreateAsset<CarAsset>();
        AssetConstraint constraint = AssetConstraint
            .Include<CarAsset>(fixture.Assets)
            .Build();

        // Assert
        constraint
            .Applicable(asset.Id)
            .Should()
            .BeTrue();
    }

    [Fact(DisplayName = "Должен возвращать false в Applicable, если компонент исключен")]
    public void Should_Not_Be_Applicable_When_Excluded_Component_Exists()
    {
        // Arrange
        Asset asset = fixture.CreateAsset<CarAsset>();
        AssetConstraint constraint = AssetConstraint
            .Exclude<CarAsset>(fixture.Assets)
            .Build();

        // Assert
        constraint
            .Applicable(asset.Id)
            .Should()
            .BeFalse();
    }

    [Fact(DisplayName = "Builder должен выбрасывать исключение при добавлении дублирующегося компонента")]
    public void Builder_Should_Throw_On_Duplicate_Component()
    {
        // Arrange
        AssetConstraint.Builder builder = AssetConstraint.Include<CarAsset>(fixture.Assets);

        // Act
        Func<AssetConstraint.Builder> action = () => builder.Include<CarAsset>();

        // Assert
        action
            .Should()
            .Throw<Exception>();
    }

    [Fact(DisplayName = "Два одинаковых ограничения должны иметь одинаковый HashCode и быть равны")]
    public void Should_Implement_Equality_Correctly()
    {
        // Arrange
        AssetContext context = fixture.Assets;
        AssetConstraint constraint1 = AssetConstraint.Include<CarAsset>(context)
            .Exclude<UnitAsset>()
            .Build();

        AssetConstraint constraint2 = AssetConstraint.Include<CarAsset>(context)
            .Exclude<UnitAsset>()
            .Build();

        // Assert
        constraint1
            .Should()
            .Be(constraint2);

        constraint1.GetHashCode()
            .Should()
            .Be(constraint2.GetHashCode());
    }

    [Fact(DisplayName = "Должен корректно работать с несколькими Include компонентами")]
    public void Should_Work_With_Multiple_Includes()
    {
        // Arrange
        Asset actor = fixture.CreateAsset<CarAsset, UnitAsset>();
        AssetConstraint constraint = AssetConstraint
            .Include<CarAsset, UnitAsset>(fixture.Assets)
            .Build();

        constraint
            .Applicable(actor.Id)
            .Should()
            .BeTrue();
    }
}
