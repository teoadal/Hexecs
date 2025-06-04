using Hexecs.Actors.Systems;
using Hexecs.Utils;

namespace Hexecs.Actors;

public sealed partial class ActorContextBuilder
{
    public sealed class ParallelSystemBuilder(int order)
    {
        private readonly List<Entry<IUpdateSystem>> _systems = [];

        public ParallelSystemBuilder Add(IUpdateSystem system)
        {
            _systems.Add(new Entry<IUpdateSystem>(system));
            return this;
        }

        public ParallelSystemBuilder Create(Func<ActorContext, IUpdateSystem> system)
        {
            _systems.Add(new Entry<IUpdateSystem>(system));
            return this;
        }

        internal Func<ActorContext, IUpdateSystem> Build() => actorContext =>
        {
            var parallelSystems = _systems.ToArray(
                static (builder, ctx) => builder.Invoke(ctx),
                actorContext);

            var result = new UpdateParallelSystem(actorContext, order, parallelSystems);

            _systems.Clear();

            return result;
        };
    }
}