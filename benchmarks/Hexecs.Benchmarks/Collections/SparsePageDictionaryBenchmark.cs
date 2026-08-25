namespace Hexecs.Benchmarks.Collections;

// BenchmarkDotNet v0.15.8, Windows 11 (10.0.22621.4317/22H2/2022Update/SunValley2)
// Intel Xeon CPU E5-2697 v3 2.60GHz, 2 CPU, 56 logical and 28 physical cores
// .NET SDK 10.0.102
//   [Host]    : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
//   .NET 10.0 : .NET 10.0.2 (10.0.2, 10.0.225.61305), X64 RyuJIT x86-64-v3
//
// Job=.NET 10.0  Runtime=.NET 10.0  
//
// | Method                    | N        | Mean              | Allocated |
// |-------------------------- |--------- |------------------:|----------:|
// | Iterate_Sparse            | 10       |          7.054 ns |         - |
// | Iterate_SparsePage        | 10       |          6.878 ns |         - |
// | Iterate_Dict              | 10       |         12.023 ns |         - |
// | Contains_Sparse_Hit       | 10       |          9.285 ns |         - |
// | Contains_SparsePage_Hit   | 10       |         15.746 ns |         - |
// | Contains_Dict_Hit         | 10       |         48.268 ns |         - |
// | Contains_Sparse_Miss      | 10       |        849.891 ns |         - |
// | Contains_SparsePage_Miss  | 10       |      1,518.830 ns |         - |
// | Contains_Dict_Miss        | 10       |      4,387.313 ns |         - |
// | TryGetValue_Sparse        | 10       |         15.586 ns |         - |
// | TryGetValue_SparsePage    | 10       |         26.951 ns |         - |
// | TryGetValue_Dict          | 10       |         51.527 ns |         - |
// | AddRemoveCycle_Sparse     | 10       |        642.318 ns |         - |
// | AddRemoveCycle_SparsePage | 10       |        757.177 ns |         - |
// | AddRemoveCycle_Dict       | 10       |      1,380.607 ns |         - |
// | Iterate_Sparse            | 100      |         61.185 ns |         - |
// | Iterate_SparsePage        | 100      |         63.300 ns |         - |
// | Iterate_Dict              | 100      |        123.782 ns |         - |
// | Contains_Sparse_Hit       | 100      |         93.043 ns |         - |
// | Contains_SparsePage_Hit   | 100      |        158.254 ns |         - |
// | Contains_Dict_Hit         | 100      |        502.972 ns |         - |
// | Contains_Sparse_Miss      | 100      |        751.221 ns |         - |
// | Contains_SparsePage_Miss  | 100      |      1,328.104 ns |         - |
// | Contains_Dict_Miss        | 100      |      3,853.558 ns |         - |
// | TryGetValue_Sparse        | 100      |        133.746 ns |         - |
// | TryGetValue_SparsePage    | 100      |        275.022 ns |         - |
// | TryGetValue_Dict          | 100      |        523.184 ns |         - |
// | AddRemoveCycle_Sparse     | 100      |      6,144.933 ns |         - |
// | AddRemoveCycle_SparsePage | 100      |      7,791.588 ns |         - |
// | AddRemoveCycle_Dict       | 100      |     13,226.643 ns |         - |
// | Iterate_Sparse            | 1000     |        545.015 ns |         - |
// | Iterate_SparsePage        | 1000     |        548.579 ns |         - |
// | Iterate_Dict              | 1000     |      1,218.623 ns |         - |
// | Contains_Sparse_Hit       | 1000     |        893.312 ns |         - |
// | Contains_SparsePage_Hit   | 1000     |      1,592.796 ns |         - |
// | Contains_Dict_Hit         | 1000     |      5,101.833 ns |         - |
// | Contains_Sparse_Miss      | 1000     |        841.138 ns |         - |
// | Contains_SparsePage_Miss  | 1000     |      1,557.011 ns |         - |
// | Contains_Dict_Miss        | 1000     |      4,262.587 ns |         - |
// | TryGetValue_Sparse        | 1000     |      1,378.559 ns |         - |
// | TryGetValue_SparsePage    | 1000     |      2,631.303 ns |         - |
// | TryGetValue_Dict          | 1000     |      5,671.514 ns |         - |
// | AddRemoveCycle_Sparse     | 1000     |     59,471.171 ns |         - |
// | AddRemoveCycle_SparsePage | 1000     |     77,915.215 ns |         - |
// | AddRemoveCycle_Dict       | 1000     |    136,277.304 ns |         - |
// | Iterate_Sparse            | 100000   |     54,179.301 ns |         - |
// | Iterate_SparsePage        | 100000   |     52,604.863 ns |         - |
// | Iterate_Dict              | 100000   |    115,975.105 ns |         - |
// | Contains_Sparse_Hit       | 100000   |      1,213.075 ns |         - |
// | Contains_SparsePage_Hit   | 100000   |      3,990.269 ns |         - |
// | Contains_Dict_Hit         | 100000   |     12,338.853 ns |         - |
// | Contains_Sparse_Miss      | 100000   |        873.536 ns |         - |
// | Contains_SparsePage_Miss  | 100000   |      1,536.765 ns |         - |
// | Contains_Dict_Miss        | 100000   |      6,450.297 ns |         - |
// | TryGetValue_Sparse        | 100000   |      4,264.815 ns |         - |
// | TryGetValue_SparsePage    | 100000   |      7,288.517 ns |         - |
// | TryGetValue_Dict          | 100000   |     13,796.095 ns |         - |
// | AddRemoveCycle_Sparse     | 100000   |     67,421.741 ns |         - |
// | AddRemoveCycle_SparsePage | 100000   |    123,045.159 ns |         - |
// | AddRemoveCycle_Dict       | 100000   |    128,143.984 ns |         - |
// | Iterate_Sparse            | 1000000  |    502,558.740 ns |         - |
// | Iterate_SparsePage        | 1000000  |    507,753.446 ns |         - |
// | Iterate_Dict              | 1000000  |  1,306,812.264 ns |         - |
// | Contains_Sparse_Hit       | 1000000  |      4,323.616 ns |         - |
// | Contains_SparsePage_Hit   | 1000000  |     24,987.266 ns |         - |
// | Contains_Dict_Hit         | 1000000  |     24,568.160 ns |         - |
// | Contains_Sparse_Miss      | 1000000  |        846.263 ns |         - |
// | Contains_SparsePage_Miss  | 1000000  |      1,478.259 ns |         - |
// | Contains_Dict_Miss        | 1000000  |      6,356.062 ns |         - |
// | TryGetValue_Sparse        | 1000000  |     20,006.167 ns |         - |
// | TryGetValue_SparsePage    | 1000000  |     37,532.782 ns |         - |
// | TryGetValue_Dict          | 1000000  |     24,830.077 ns |         - |
// | AddRemoveCycle_Sparse     | 1000000  |    124,740.165 ns |         - |
// | AddRemoveCycle_SparsePage | 1000000  |    585,653.058 ns |         - |
// | AddRemoveCycle_Dict       | 1000000  |    136,532.017 ns |         - |
// | Iterate_Sparse            | 10000000 |  6,383,516.042 ns |         - |
// | Iterate_SparsePage        | 10000000 |  6,266,198.828 ns |         - |
// | Iterate_Dict              | 10000000 | 19,314,062.188 ns |         - |
// | Contains_Sparse_Hit       | 10000000 |      8,714.852 ns |         - |
// | Contains_SparsePage_Hit   | 10000000 |     38,036.706 ns |         - |
// | Contains_Dict_Hit         | 10000000 |     53,292.242 ns |         - |
// | Contains_Sparse_Miss      | 10000000 |        861.005 ns |         - |
// | Contains_SparsePage_Miss  | 10000000 |      1,514.329 ns |         - |
// | Contains_Dict_Miss        | 10000000 |      9,843.414 ns |         - |
// | TryGetValue_Sparse        | 10000000 |     44,180.863 ns |         - |
// | TryGetValue_SparsePage    | 10000000 |     64,472.113 ns |         - |
// | TryGetValue_Dict          | 10000000 |     56,616.146 ns |         - |
// | AddRemoveCycle_Sparse     | 10000000 |    206,125.551 ns |         - |
// | AddRemoveCycle_SparsePage | 10000000 |    891,220.111 ns |         - |
// | AddRemoveCycle_Dict       | 10000000 |    201,447.264 ns |         - |

[SimpleJob(RuntimeMoniker.Net10_0)]
[MeanColumn, MemoryDiagnoser]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD", "Count")]
[JsonExporterAttribute.Full]
[JsonExporterAttribute.FullCompressed]
[BenchmarkCategory("Collections")]
public class SparsePageDictionaryBenchmark
{
    private SparsePageDictionary<int> _sparsePage = null!;
    private SparsePageDictionary<int> _sparsePageCycle = null!;

    private SparseDictionary<int> _sparse = null!;
    private SparseDictionary<int> _sparseCycle = null!;

    private Dictionary<uint, int> _dict = null!;
    private Dictionary<uint, int> _dictCycle = null!;

    private uint[] _keys = null!;
    private uint[] _lookupKeys = null!;
    private uint[] _missingKeys = null!;

    [Params(10, 100, 1_000, 100_000, 1_000_000, 10_000_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var keyRange = Math.Max(N * 10, 1_000);
        _keys = Enumerable.Range(0, N * 2)
            .Select(_ => (uint)Random.Shared.Next(0, keyRange))
            .Distinct()
            .Take(N)
            .ToArray();

        _lookupKeys = _keys
            .OrderBy(_ => Random.Shared.Next())
            .Take(Math.Min(1000, _keys.Length))
            .ToArray();

        var present = new bool[keyRange];
        for (var i = 0; i < _keys.Length; i++)
            present[_keys[i]] = true;

        var missCount = 1000;
        var missing = new uint[missCount];
        var written = 0;

        for (var k = 0; k < keyRange && written < missCount; k++)
        {
            if (!present[k]) missing[written++] = (uint)k;
        }

        if (written < missCount) Array.Resize(ref missing, written);
        _missingKeys = missing;

        _sparsePage = new SparsePageDictionary<int>(denseCapacity: N);
        _sparsePageCycle = new SparsePageDictionary<int>(denseCapacity: N);

        _sparse = new SparseDictionary<int>(capacity: keyRange);
        _sparseCycle = new SparseDictionary<int>(capacity: keyRange);

        _dict = new Dictionary<uint, int>(capacity: N);
        _dictCycle = new Dictionary<uint, int>(capacity: N);

        const int value = 42;
        foreach (var key in _keys)
        {
            _sparsePage.Add(key, value);
            _sparsePageCycle.Add(key, value);

            _sparse.Add(key, value);
            _sparseCycle.Add(key, value);

            _dict.Add(key, value);
            _dictCycle.Add(key, value);
        }

        _sparsePageCycle.Clear();
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
    public int Iterate_SparsePage()
    {
        var sum = 0;
        foreach (var entry in _sparsePage)
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
    public bool Contains_SparsePage_Hit()
    {
        var found = false;
        foreach (var key in _lookupKeys)
        {
            found = _sparsePage.Contains(key);
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
    public bool Contains_SparsePage_Miss()
    {
        var found = false;
        foreach (var key in _missingKeys)
        {
            found = _sparsePage.Contains(key);
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
    public int TryGetValue_SparsePage()
    {
        var sum = 0;
        foreach (var key in _lookupKeys)
        {
            if (_sparsePage.TryGetValue(key, out var value))
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
    public int AddRemoveCycle_SparsePage()
    {
        var dict = _sparsePageCycle;
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