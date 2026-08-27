using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;

BenchmarkRunner.Run<ActorCheckComponentExistsBenchmark>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
