// See https://aka.ms/new-console-template for more information

using Hexecs.Monogame.Features;
using Hexecs.Worlds;

var world = new WorldBuilder()
    .DebugWorld()
    .ConfigureFeatures()
    .DefaultActorContext(defaultContext => defaultContext
        .AddFeatures());


Console.WriteLine("Hello, World!");