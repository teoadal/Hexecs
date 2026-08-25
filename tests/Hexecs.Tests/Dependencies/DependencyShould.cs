using Hexecs.Dependencies;

namespace Hexecs.Tests.Dependencies;

public class DependencyTests
{
    [Fact]
    public void Resolve_WithInstance_ReturnsInstance()
    {
        // Arrange
        var contract = typeof(string);
        var lifetime = DependencyLifetime.Transient;
        var instance = "Test Instance";
        var dependency = new Dependency(lifetime, contract, instance);

        // Act
        var resolvedInstance = dependency.Resolve(Mock.Of<IDependencyProvider>());

        // Assert
        resolvedInstance.Should().BeSameAs(instance);
    }

    [Fact]
    public void Resolve_WithResolver_ReturnsResolvedInstance()
    {
        // Arrange
        var contract = typeof(string);
        var lifetime = DependencyLifetime.Transient;
        var resolverResult = "Test Resolved Instance";
        var resolver = new Func<IDependencyProvider, object>(provider => resolverResult);

        var dependency = new Dependency(lifetime, contract, resolver);

        // Act
        var resolvedInstance = dependency.Resolve(Mock.Of<IDependencyProvider>());

        // Assert
        resolvedInstance
            .Should()
            .Be(resolverResult);
    }

    [Fact]
    public void Resolve_WithNullResolver_And_Null_Instance_ThrowsException()
    {
        // Arrange
        var contract = typeof(string);
        var lifetime = DependencyLifetime.Transient;
        var dependency = new Dependency(lifetime, contract, null!);

        // Act & Assert
        dependency
            .Invoking(d => d.Resolve(Mock.Of<IDependencyProvider>()))
            .Should().Throw<Exception>();
    }

    [Fact]
    public void Dependency_CtorWithInstance_SetsPropertiesCorrectly()
    {
        // Arrange
        var contract = typeof(string);
        var lifetime = DependencyLifetime.Transient;
        var instance = "Test Instance";

        // Act
        var dependency = new Dependency(lifetime, contract, instance);

        // Assert
        dependency.Contract.Should().Be(contract);
        dependency.Lifetime.Should().Be(lifetime);
        dependency.Resolver.Should().BeNull();
        dependency.Instance.Should().BeSameAs(instance);
    }

    [Fact]
    public void Dependency_CtorWithResolver_SetsPropertiesCorrectly()
    {
        // Arrange
        var contract = typeof(string);
        var lifetime = DependencyLifetime.Transient;
        var resolver = new Func<IDependencyProvider, object>(provider => "Resolved Instance");

        // Act
        var dependency = new Dependency(lifetime, contract, resolver);

        // Assert
        dependency.Contract.Should().Be(contract);
        dependency.Lifetime.Should().Be(lifetime);
        dependency.Resolver.Should().BeSameAs(resolver);
        dependency.Instance.Should().BeNull();
    }
}