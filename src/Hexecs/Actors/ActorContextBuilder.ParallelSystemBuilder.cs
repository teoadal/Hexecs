using Hexecs.Actors.Systems;
using Hexecs.Threading;

namespace Hexecs.Actors;

public sealed partial class ActorContextBuilder
{
    public sealed class ParallelSystemBuilder(int order)
    {
        private readonly List<Entry<IUpdateSystem>> _systems = [];
        private IParallelWorker? _worker;

        public ParallelSystemBuilder Add(IUpdateSystem system)
        {
            _systems.Add(new Entry<IUpdateSystem>(system));
            return this;
        }

        public ParallelSystemBuilder Add(params Span<IUpdateSystem> systems)
        {
            foreach (var updateSystem in systems)
            {
                Add(updateSystem);
            }

            return this;
        }
        
        public ParallelSystemBuilder Add(IEnumerable<IUpdateSystem> systems)
        {
            foreach (var updateSystem in systems)
            {
                Add(updateSystem);
            }

            return this;
        }

        public ParallelSystemBuilder Create(Func<ActorContext, IUpdateSystem> system)
        {
            _systems.Add(new Entry<IUpdateSystem>(system));
            return this;
        }

        public ParallelSystemBuilder Worker(IParallelWorker worker)
        {
            _worker = worker;
            return this;
        }

        internal Func<ActorContext, IUpdateSystem> Build() => actorContext =>
        {
            var parallelSystems = _systems.ToArray(
                static (builder, ctx) => builder.Invoke(ctx),
                actorContext);

            var worker = _worker ?? actorContext.GetService<IParallelWorker>();
            if (worker == null) ActorError.ParallelWorkerNotRegistered();

            var result = new ParallelSystem(actorContext, order, parallelSystems, worker);

            _systems.Clear();

            return result;
        };
    }
}