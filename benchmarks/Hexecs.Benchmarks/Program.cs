using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;
using Hexecs.Benchmarks.Collections;

BenchmarkRunner.Run<SparsePageDictionaryBenchmark>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
