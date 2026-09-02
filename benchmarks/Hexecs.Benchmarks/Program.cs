using BenchmarkDotNet.Running;

using Hexecs.Benchmarks.Actors;
using Hexecs.Benchmarks.Units;

BenchmarkRunner.Run<ActorCreateAddComponentsDestroyBenchmark>();
//BenchmarkRunner.Run<QueueBenchmark>(new DebugBuildConfig());
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

//Playground.Do();
