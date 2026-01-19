using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;

BenchmarkRunner.Run<ActorCreateAddComponentsDestroyBenchmark>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);