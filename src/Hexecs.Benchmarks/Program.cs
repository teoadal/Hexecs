using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;

//BenchmarkRunner.Run<CreateAddComponentsDestroyBenchmark>();
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);