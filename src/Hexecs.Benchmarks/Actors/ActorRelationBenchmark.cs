using System.Buffers;
using Friflo.Engine.ECS;
using Hexecs.Benchmarks.Mocks.ActorComponents;
using Hexecs.Worlds;

namespace Hexecs.Benchmarks.Actors;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//     .NET SDK 10.0.100
//     [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//     .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
//     | Method | Count | Mean          | Ratio | Allocated | Alloc Ratio |
//     |------- |------ |--------------:|------:|----------:|------------:|
//     | Hexecs | 10    |      13.32 us |  1.00 |         - |          NA |
//     | FriFlo | 10    |      15.66 us |  1.18 |         - |          NA |
//     |        |       |               |       |           |             |
//     | Hexecs | 100   |   1,929.54 us |  1.00 |         - |          NA |
//     | FriFlo | 100   |   2,301.57 us |  1.19 |         - |          NA |
//     |        |       |               |       |           |             |
//     | Hexecs | 1000  | 604,757.61 us |  1.00 |         - |          NA |
//     | FriFlo | 1000  | 807,112.05 us |  1.33 |         - |          NA |
//
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
    [Params(10, 100, 1_000)] public int Count;

    private ActorContext _actorContext = null!;
    private Actor[] _actorBuffer = null!;
    private ActorFilter<Employee> _employeeFilter = null!;
    private ActorFilter<Employer> _employerFilter = null!;
    private World _world = null!;

    private EntityStore _frifloWorld = null!;
    private Entity[] _frifloBuffer = null!;
    private ArchetypeQuery<Employee> _frifloEmployees = null!;
    private ArchetypeQuery<Employer> _frifloEmployers = null!;

    [Benchmark(Baseline = true)]
    public int Hexecs()
    {
        // Часть 1: Наполнение
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
        var buffer = _actorBuffer;

        // Часть 2: Удаление
        foreach (var employer in _employerFilter)
        {
            var relations = employer.Relations<EmployeeAgreement>();
            var i = 0;

            foreach (var relation in relations)
            {
                buffer[i++] = relation;
            }

            for (var j = 0; j < i; j++)
            {
                if (employer.RemoveRelation<EmployeeAgreement>(buffer[j]))
                {
                    result++;
                }
            }
        }

        return result;
    }

    [Benchmark]
    public int FriFlo()
    {
        // Часть 1: Наполнение
        using (var employeeEnumerator = _frifloEmployees.Entities.GetEnumerator())
        {
            foreach (var employer in _frifloEmployers.Entities)
            {
                for (var i = 0; i < Count; i++)
                {
                    if (!employeeEnumerator.MoveNext()) break;
                    var employee = employeeEnumerator.Current;
                    employer.AddRelation(new EmployeeAgreement { Salary = i, Target = employee });
                }
            }
        }

        var result = 0;
        var buffer = _frifloBuffer;

        // Часть 2: Удаление
        foreach (var employer in _frifloEmployers.Entities)
        {
            // Получаем все связи данного типа у сущности
            var relations = employer.GetRelations<EmployeeAgreement>();
            var i = 0;

            foreach (var relation in relations)
            {
                buffer[i++] = relation.Target;
            }

            for (var j = 0; j < i; j++)
            {
                if (employer.RemoveRelation<EmployeeAgreement>(buffer[j]))
                {
                    result++;
                }
            }
        }

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
        _actorBuffer = new Actor[Count];
        _employeeFilter = _actorContext.Filter<Employee>();
        _employerFilter = _actorContext.Filter<Employer>();

        _frifloWorld = new EntityStore();
        _frifloBuffer = new Entity[Count];
        _frifloEmployees = _frifloWorld.Query<Employee>();
        _frifloEmployers = _frifloWorld.Query<Employer>();

        for (var i = 0; i < Count; i++)
        {
            var employer = _actorContext.CreateActor();
            employer.Add(new Employer());

            _frifloWorld.CreateEntity(new Employer());

            for (var y = 0; y < Count; y++)
            {
                var employee = _actorContext.CreateActor();
                employee.Add(new Employee());

                _frifloWorld.CreateEntity(new Employee());
            }
        }
    }
}