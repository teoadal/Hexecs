using System.Diagnostics;
using Hexecs.Tests.Mocks;
using Hexecs.Worlds;

namespace Hexecs.Tests.Worlds;

public sealed class WordDependencyShould
{
    private readonly World _world = new WorldBuilder()
        .UseSingleton(_ => new Singleton())
        .UseSingleton(_ => new Singleton())
        .UseScoped(_ => new Scoped())
        .UseTransient(_ => new Transient())
        .Build();

    [Fact]
    public void NotThrowWhenDependencyNotRegistered()
    {
        var dependency = _world
            .Invoking(w => w.GetService<Process>())
            .Should()
            .NotThrow()
            .Which;

        dependency
            .Should()
            .BeNull();
    }
    
    [Fact]
    public void ResolveSingleton()
    {
        var dependency = _world.Invoking(w => w.GetService<Singleton>())
            .Should()
            .NotThrow()
            .Which;

        dependency
            .Should()
            .NotBeNull();
    }
    
    [Fact]
    public void ResolveSingletonArray()
    {
        var array = _world.Invoking(w => w.GetServices<Singleton>())
            .Should()
            .NotThrow()
            .Which;

        array
            .Should()
            .OnlyHaveUniqueItems();
    }
    
    [Fact]
    public void ResolveSameSingletonArray()
    {
        var array1 = _world.GetServices<Singleton>();
        var array2 = _world.GetServices<Singleton>();

        array1
            .Should()
            .BeSameAs(array2);
    }
    
    [Fact]
    public void ResolveSameSingleton()
    {
        var dependency1 = _world.GetService<Singleton>();
        var dependency2 = _world.GetService<Singleton>();

        dependency1
            .Should()
            .Be(dependency2);
    }
    
    [Fact]
    public void ResolveSameScoped()
    {
        var dependency1 = _world.GetService<Scoped>();
        var dependency2 = _world.GetService<Scoped>();

        dependency1
            .Should()
            .Be(dependency2);
    }
    
    [Fact]
    public void ResolveScoped()
    {
        var dependency = _world.Invoking(w => w.GetService<Scoped>())
            .Should()
            .NotThrow()
            .Which;

        dependency
            .Should()
            .NotBeNull();
    }
    
    [Fact]
    public void ResolveTransient()
    {
        var dependency = _world.Invoking(w => w.GetService<Transient>())
            .Should()
            .NotThrow()
            .Which;

        dependency
            .Should()
            .NotBeNull();
    }
    
    [Fact]
    public void ResolveNotSameTransient()
    {
        var dependency1 = _world.GetService<Transient>();
        var dependency2 = _world.GetService<Transient>();

        dependency1
            .Should()
            .NotBe(dependency2);
    }
}