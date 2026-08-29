using Hexecs.Benchmarks.Mocks.ActorComponents;

using Hexecsm.Worlds;

using ActorId = Hexecsm.ActorId;

// BenchmarkRunner.Run<UpdateSystemWithParallelWorkerBenchmark>();

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

var world = new World(256);

ActorId actorId = world.CreateActor();
world.AddComponent(actorId, new Attack { Value = 1 });
world.AddComponent(actorId, new Attack { Value = 123 });
world.AddComponent(actorId, new Defence { Value = 2 });

Check(actorId, world);

world.Update();

Check(actorId, world);

return;

static void Check(ActorId actorId, World world)
{
    Print("Is Alive", world.IsAlive(actorId));
    Print($"Has {nameof(Attack)}", world.HasComponent<Attack>(actorId));
    Print($"Has {nameof(Defence)}", world.HasComponent<Defence>(actorId));
}

static void Print(string message, object value)
{
    Console.WriteLine($"{message}: {value}");
}
