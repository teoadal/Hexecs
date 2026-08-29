using BenchmarkDotNet.Running;

using Hexecs.Benchmarks.Actors;
using Hexecs.Benchmarks.Mocks.ActorComponents;

using Hexecsm.Filters;
using Hexecsm.Worlds;

using ActorId = Hexecsm.ActorId;

BenchmarkRunner.Run<ActorFilter2EnumerationBenchmark>();

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// using var world = new World(256);
//
// Filter<Attack, Defence> filter = world.GetFilter<Attack, Defence>();
//
// ActorId actorId = world.CreateActor();
// world.AddComponent(actorId, new Attack { Value = 1 });
// world.AddComponent(actorId, new Defence { Value = 2 });
//
// Check(actorId, world);
//
// world.Update();
//
//
// Check(actorId, world);
//
// foreach (Hexecsm.ActorRef<Attack, Defence> actorRef in filter)
// {
//     Console.WriteLine(actorRef.Id.Value);
// }
//
// return;
//
// static void Check(ActorId actorId, World world)
// {
//     Print("Is Alive", world.IsAlive(actorId));
//     Print($"Has {nameof(Attack)}", world.HasComponent<Attack>(actorId));
//     Print($"Has {nameof(Defence)}", world.HasComponent<Defence>(actorId));
// }
//
// static void Print(string message, object value)
// {
//     Console.WriteLine($"{message}: {value}");
// }
