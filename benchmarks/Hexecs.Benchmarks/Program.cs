using BenchmarkDotNet.Running;

using Hexecs.Benchmarks.Actors;
using Hexecs.Benchmarks.Collections;
using Hexecs.Benchmarks.Units;


BenchmarkRunner.Run<QueueBenchmark>();
//BenchmarkRunner.Run<QueueBenchmark>(new DebugBuildConfig());
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

//Playground.Do();
