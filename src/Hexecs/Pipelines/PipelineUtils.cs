
namespace Hexecs.Pipelines;

internal static class PipelineUtils
{
    public static Type GetCommandType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type commandHandlerType)
    {
        foreach (Type contract in commandHandlerType.GetInterfaces())
        {
            if (!contract.IsGenericType)
            {
                continue;
            }

            Type genericTypeDefinition = contract.GetGenericTypeDefinition();
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