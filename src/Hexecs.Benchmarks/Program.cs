using BenchmarkDotNet.Running;


//BenchmarkRunner.Run<UpdateSystemWithParallelWorkerBenchmark>();
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);