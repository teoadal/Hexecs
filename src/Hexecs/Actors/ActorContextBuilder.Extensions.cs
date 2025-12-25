using Hexecs.Dependencies;
using Hexecs.Pipelines;
using Hexecs.Pipelines.Commands;

namespace Hexecs.Actors;

public static class ActorContextBuilderExtensions
{
    extension(ActorContextBuilder builder)
    {
        /// <summary>
        /// Создаёт строитель актёров указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип строителя актёров.</typeparam>
        /// <returns>Этот же экземпляр ActorContextBuilder для цепочки вызовов.</returns>
        public ActorContextBuilder CreateBuilder<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
                                        DynamicallyAccessedMemberTypes.PublicConstructors)]
            T>() where T : class, IActorBuilder
        {
            builder.CreateBuilder(static ctx => (IActorBuilder)ctx.Activate(typeof(T)));
            return builder;
        }


        /// <summary>
        /// Регистрирует метод создания обработчика команды указанного типа.
        /// </summary>
        /// <remarks>
        /// Использует рефлексию.
        /// </remarks>
        public ActorContextBuilder CreateCommandHandler<
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces |
                                            DynamicallyAccessedMemberTypes.PublicConstructors)]
                THandler>
            ()
            where THandler : class, ICommandHandler
        {
            var commandType = PipelineUtils.GetCommandType(typeof(THandler));
            var commandId = CommandType.GetId(commandType);

            builder.InsertCommandHandlerEntry(
                commandId,
                commandType,
                new ActorContextBuilder.Entry<ICommandHandler>(static ctx =>
                    (ICommandHandler)ctx.Activate(typeof(THandler))));

            return builder;
        }

        /// <summary>
        /// Регистрирует метод создания системы отрисовки актёров.
        /// </summary>
        /// <remarks>
        /// Использует рефлексию.
        /// </remarks>
        public ActorContextBuilder CreateDrawSystem<
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TSystem>
            () where TSystem : class, IDrawSystem
        {
            builder.CreateDrawSystem(static ctx => (IDrawSystem)ctx.Activate(typeof(TSystem)));
            return builder;
        }

        /// <summary>
        /// Регистрирует метод создания системы обновления актёров.
        /// </summary>
        /// <remarks>
        /// Использует рефлексию.
        /// </remarks>
        public ActorContextBuilder CreateUpdateSystem<
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TSystem>
            () where TSystem : class, IUpdateSystem
        {
            builder.CreateUpdateSystem(static ctx => (IUpdateSystem)ctx.Activate(typeof(TSystem)));
            return builder;
        }
    }

    /// <summary>
    /// Регистрирует метод создания системы обновления актёров.
    /// </summary>
    /// <remarks>
    /// Использует рефлексию.
    /// </remarks>
    public static ActorContextBuilder.ParallelSystemBuilder CreateUpdateSystem<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TSystem>
        (this ActorContextBuilder.ParallelSystemBuilder builder) where TSystem : class, IUpdateSystem
    {
        builder.Create(static ctx => (IUpdateSystem)ctx.Activate(typeof(TSystem)));
        return builder;
    }
}