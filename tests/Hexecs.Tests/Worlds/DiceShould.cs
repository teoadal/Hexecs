namespace Hexecs.Tests.Worlds;

public sealed class DiceShould(WorldTestFixture fixture) : IClassFixture<WorldTestFixture>
{
    [Fact(DisplayName = "Возвращать значение в указанном пределе")]
    public void GetValueBetweenExpected()
    {
        var dice = fixture.World.Dice;
        for (var start = -100; start < 100; start++)
        {
            var end = start + fixture.Random.Next(1, 100);

            dice
                .GetNext(start, end)
                .Should().BeInRange(start, end);
        }
    }
    
    [Fact(DisplayName = "Возвращать значение равное пределу, если пределы одинаковы")]
    public void GetValueEqualToLimitWhenLimitsAreSame()
    {
        var dice = fixture.World.Dice;
        // Получаем случайное значение, которое будет использовано как start и end
        var value = fixture.Random.Next(-100, 100); 

        dice.GetNext(value, value)
            .Should().Be(value);
    }
}