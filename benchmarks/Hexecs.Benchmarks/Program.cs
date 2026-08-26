using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;
using Hexecs.Benchmarks.Collections;

BenchmarkRunner.Run<ActorFilter2EnumerationBenchmark>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
