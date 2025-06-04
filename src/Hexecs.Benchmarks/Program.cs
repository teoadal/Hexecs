using BenchmarkDotNet.Running;
using Hexecs.Benchmarks;
using Hexecs.Benchmarks.Actors;
using Hexecs.Benchmarks.Collections;

// var a = new ActorFilter3EnumerartionBenchmark();
// a.Setup();
// for (int i = 0; i < 1_000_000; i++)
// {
//     a.Hexecs();
// }
//
// a.Cleanup();

//BenchmarkRunner.Run<DictionaryBenchmark>();

BenchmarkRunner.Run<CreateAddComponentsDestroyBenchmark>();