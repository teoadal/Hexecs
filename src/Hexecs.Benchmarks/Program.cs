using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;

BenchmarkRunner.Run<UpdateSystemWithParallelWorkerBenchmark>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);