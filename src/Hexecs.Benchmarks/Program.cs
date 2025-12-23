using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;


BenchmarkRunner.Run<ActorFilter2EnumerationBenchmark>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);