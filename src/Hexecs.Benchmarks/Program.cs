using BenchmarkDotNet.Running;
using Hexecs.Benchmarks.Actors;

BenchmarkRunner.Run<UpdateSystemWithParallelWorkerBenchmark>();