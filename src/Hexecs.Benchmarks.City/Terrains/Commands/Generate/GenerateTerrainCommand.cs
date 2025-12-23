using System.Runtime.CompilerServices;
using Hexecs.Pipelines;

namespace Hexecs.Benchmarks.Map.Terrains.Commands.Generate;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct GenerateTerrainCommand : ICommand;