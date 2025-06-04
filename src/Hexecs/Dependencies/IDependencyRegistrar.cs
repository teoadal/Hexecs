namespace Hexecs.Dependencies;

public interface IDependencyRegistrar
{
    void TryRegister(IDependencyCollection services);
}