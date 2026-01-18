using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;

BenchmarkRunner.Run<ActorHierarchyBenchmark>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);