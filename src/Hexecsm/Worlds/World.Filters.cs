using Hexecsm.Filters;

namespace Hexecsm.Worlds;

public sealed partial class World
{
    private readonly Dictionary<Type, IFilter> _filters = [];
    private readonly Lock _filtersLock = new Lock();

    public Filter<T1, T2> GetFilter<T1, T2>()
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        Type key = typeof(Filter<T1, T2>);

        using (_filtersLock.EnterScope())
        {
            if (_filters.TryGetValue(key, out IFilter? existsFilter))
            {
                return (Filter<T1, T2>)existsFilter;
            }

            var filter = new Filter<T1, T2>(
                componentPool1: GetOrAddComponentPool<T1>(),
                componentPool2: GetOrAddComponentPool<T2>(),
                _eventBus);

            _filters[key] = filter;

            return filter;
        }
    }
}
