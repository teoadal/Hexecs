using AutoFixture;
using Hexecs.Utils;

namespace Hexecs.Tests;

public abstract class BaseFixture
{
    public Fixture Fixture => field ??= new Fixture();

    public readonly Random Random = new();

    public T[] CreateArray<T>(Func<int, T> factory) => Enumerable
        .Range(0, RandomInt(10, 100))
        .Select(factory)
        .ToArray();

    public T[] CreateArray<T>(int length, Func<int, T> factory) => Enumerable
        .Range(0, length)
        .Select(factory)
        .ToArray();

    public int RandomInt() => Random.Next();

    public int RandomInt(int from, int to) => Random.Next(from, to);

    public string RandomString(int length = 12) => StringUtils.GetRandom(length);
    
    public int RandomPositiveInt(uint minValue = 0, uint maxValue = int.MaxValue)
    {
        return Random.Next((int)minValue, (int)maxValue);
    }
}