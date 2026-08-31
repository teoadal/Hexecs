using BenchmarkDotNet.Running;

using Hexecs.Benchmarks.Collections;

// var q = new ThreadLocalQueue<int>(32);
//
// for (var i = 0; i < 3128; i++)
// {
//     q.Enqueue(i);
// }
//
// foreach (ThreadLocalQueue<int>.LocalQueue batch in q.GetBatches())
// {
//     foreach (int i in batch.AsSpan())
//     {
//         Console.WriteLine(i);
//     }
//
//     batch.Clear();
// }

BenchmarkRunner.Run<QueueBenchmark>();
//BenchmarkRunner.Run<QueueBenchmark>(new DebugBuildConfig());
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

//Playground.Do();
