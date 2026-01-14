using System.Buffers;
using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, macOS Tahoe 26.2 (25C56) [Darwin 25.2.0]
// Apple M3 Max, 1 CPU, 16 logical and 16 physical cores                                                                                                 
//     .NET SDK 10.0.101                                                                                                                                     
//     [Host]    : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a                                                                            
//     .NET 10.0 : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a                                                                            
//                                                                                                                                                       
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method | Count | Mean           | Allocated |
//     |------- |------ |---------------:|----------:|
//     | Do     | 100   |       798.9 us |         - |                                                                                                       
//     | Do     | 1000  |   261,046.6 us |         - |
//     | Do     | 2000  | 2,049,070.5 us |         - |

[SimpleJob(RuntimeMoniker.Net10_0)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Actors")]
public class ActorRelationBenchmark
{
    [Params(100, 1_000, 2_000)] public int Count;

    private ActorContext _actorContext = null!;
    private ActorFilter<Employee> _employeeFilter = null!;
    private ActorFilter<Employer> _employerFilter = null!;
    private World _world = null!;

    [Benchmark]
    public int Do()
    {
        // Часть 1: Наполнение (тут всё отлично)
        using (var employeeEnumerator = _employeeFilter.GetEnumerator())
        {
            foreach (var employer in _employerFilter)
            {
                for (var i = 0; i < Count; i++)
                {
                    if (!employeeEnumerator.MoveNext()) break;
                    var employee = employeeEnumerator.Current;
                    employer.AddRelation(employee, new EmployeeAgreement { Salary = i });
                }
            }
        }

        var result = 0;
        var buffer = ArrayPool<uint>.Shared.Rent(Count);

        // Часть 2: Удаление
        foreach (var employer in _employerFilter)
        {
            var relations = employer.Relations<EmployeeAgreement>();
            var i = 0;

            // Сначала копируем ID во временный буфер, чтобы не ломать итератор
            foreach (var relation in relations)
            {
                buffer[i++] = relation.Id;
            }

            // Теперь спокойно удаляем
            for (var j = 0; j < i; j++)
            {
                if (_actorContext.RemoveRelation<EmployeeAgreement>(employer.Id, buffer[j]))
                {
                    result++;
                }
            }
        }

        ArrayPool<uint>.Shared.Return(buffer);
        return result;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
        _world = null!;
    }

    [GlobalSetup]
    public void Setup()
    {
        _world = new WorldBuilder().Build();

        _actorContext = _world.Actors;
        _employeeFilter = _actorContext.Filter<Employee>();
        _employerFilter = _actorContext.Filter<Employer>();

        for (var i = 0; i < Count; i++)
        {
            var employer = _actorContext.CreateActor();
            employer.Add(new Employer());

            for (var y = 0; y < Count; y++)
            {
                var employee = _actorContext.CreateActor();
                employee.Add(new Employee());
            }
        }
    }
}