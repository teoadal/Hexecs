
namespace Hexecs.Pipelines;

internal static class PipelineUtils
{
    public static Type GetCommandType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type commandHandlerType)
    {
        foreach (var contract in commandHandlerType.GetInterfaces())
        {
            if (!contract.IsGenericType) continue;
            var genericTypeDefinition = contract.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(ICommandHandler<>) ||
                genericTypeDefinition == typeof(ICommandHandler<,>))
            {
                return contract.GetGenericArguments()[0];    
            }
        }
        
        PipelineError.CommandHandlerNotImplementedHandlerInterface(commandHandlerType);
        return null;
    }
}