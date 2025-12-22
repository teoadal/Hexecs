namespace Hexecs.Benchmarks.Collections;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
//    .NET SDK 10.0.100
//    [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
//    .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3

// Job=.NET 10.0  Runtime=.NET 10.0  
//
//    | Method                | N    | Mean         | Allocated |
//    |---------------------- |----- |-------------:|----------:|
//    | Iterate_Sparse        | 1000 |     509.2 ns |         - |
//    | Iterate_Dict          | 1000 |   1,062.4 ns |         - |
//    | Iterate_Sparse_Span   | 1000 |     362.0 ns |         - |
//    | Contains_Sparse_Hit   | 1000 |   3,184.4 ns |         - |
//    | Contains_Dict_Hit     | 1000 |   4,950.2 ns |         - |
//    | Contains_Sparse_Miss  | 1000 |     689.1 ns |         - |
//    | Contains_Dict_Miss    | 1000 |   4,156.2 ns |         - |
//    | TryGetValue_Sparse    | 1000 |   5,208.9 ns |         - |
//    | TryGetValue_Dict      | 1000 |   5,057.5 ns |         - |
//    | AddRemoveCycle_Sparse | 1000 |  94,951.8 ns |         - |
//    | AddRemoveCycle_Dict   | 1000 | 155,528.9 ns |         - |

[SimpleJob(RuntimeMoniker.Net10_0)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD", "Count")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Collections")]
public class SparsePageDictionaryBenchmark
{
    private SparsePageDictionary<int> _sparse = null!;
    private SparsePageDictionary<int> _sparseCycle = null!;
    private Dictionary<uint, int> _dict = null!;
    private Dictionary<uint, int> _dictCycle = null!;
    private uint[] _keys = null!;
    private uint[] _lookupKeys = null!;
    private uint[] _missingKeys = null!;

    [Params(500)] 
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        // Генерируем уникальные ключи в диапазоне 0–1M
        _keys = Enumerable.Range(0, N)
            .Select(_ => (uint)Random.Shared.Next(0, 1_000_000))
            .Distinct()
            .Take(N)
            .ToArray();

        // Ключи для поиска (существующие)
        _lookupKeys = _keys
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(1000, _keys.Length))
            .ToArray();

        // Ключи которых нет в словаре
        _missingKeys = Enumerable.Range(0, 1000)
            .Select(_ => (uint)Random.Shared.Next(2_000_000, 3_000_000))
            .ToArray();

        _sparse = new SparsePageDictionary<int>(denseCapacity: N);
        _sparseCycle = new SparsePageDictionary<int>(denseCapacity: N);
        _dict = new Dictionary<uint, int>(N);
        _dictCycle = new Dictionary<uint, int>(N);

        foreach (var key in _keys)
        {
            _sparse.Add(key, 42);
            _sparseCycle.Add(key, 42);
            _dict[key] = 42;
            _dictCycle[key] = 42;
        }

        _sparseCycle.Clear();
        _dictCycle.Clear();
    }

    // ===== ИТЕРАЦИЯ =====

    [Benchmark]
    public int Iterate_Sparse()
    {
        var sum = 0;
        foreach (var entry in _sparse)
        {
            sum += entry;
        }

        return sum;
    }

    [Benchmark]
    public int Iterate_Dict()
    {
        var sum = 0;
        foreach (var kv in _dict)
        {
            sum += kv.Value;
        }

        return sum;
    }

    [Benchmark]
    public int Iterate_Sparse_Span()
    {
        var sum = 0;
        var values = _sparse.Values;
        for (var i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    // ===== CONTAINS (HIT) =====

    [Benchmark]
    public bool Contains_Sparse_Hit()
    {
        var found = false;
        foreach (var key in _lookupKeys)
        {
            found = _sparse.Contains(key);
        }

        return found;
    }

    [Benchmark]
    public bool Contains_Dict_Hit()
    {
        var found = false;
        foreach (var key in _lookupKeys)
        {
            found = _dict.ContainsKey(key);
        }

        return found;
    }

    // ===== CONTAINS (MISS) =====

    [Benchmark]
    public bool Contains_Sparse_Miss()
    {
        var found = false;
        foreach (var key in _missingKeys)
        {
            found = _sparse.Contains(key);
        }

        return found;
    }

    [Benchmark]
    public bool Contains_Dict_Miss()
    {
        var found = false;
        foreach (var key in _missingKeys)
        {
            found = _dict.ContainsKey(key);
        }

        return found;
    }

    // ===== TRY GET VALUE =====

    [Benchmark]
    public int TryGetValue_Sparse()
    {
        var sum = 0;
        foreach (var key in _lookupKeys)
        {
            if (_sparse.TryGetValue(key, out var value))
            {
                sum += value;
            }
        }

        return sum;
    }

    [Benchmark]
    public int TryGetValue_Dict()
    {
        var sum = 0;
        foreach (var key in _lookupKeys)
        {
            if (_dict.TryGetValue(key, out var value))
            {
                sum += value;
            }
        }

        return sum;
    }

    // ===== ADD + REMOVE CYCLE =====

    [Benchmark]
    public int AddRemoveCycle_Sparse()
    {
        var dict = _sparseCycle;
        var count = 0;

        // Симуляция ECS: добавляем/удаляем entity из фильтра
        for (var i = 0; i < 10; i++)
        {
            foreach (var key in _lookupKeys)
            {
                dict.Add(key, 42);
            }

            count += dict.Count;

            foreach (var key in _lookupKeys)
            {
                dict.Remove(key);
            }
        }

        return count;
    }

    [Benchmark]
    public int AddRemoveCycle_Dict()
    {
        var dict = _dictCycle;
        var count = 0;

        for (var i = 0; i < 10; i++)
        {
            foreach (var key in _lookupKeys)
            {
                dict[key] = 42;
            }

            count += dict.Count;

            foreach (var key in _lookupKeys)
            {
                dict.Remove(key);
            }
        }

        return count;
    }
}