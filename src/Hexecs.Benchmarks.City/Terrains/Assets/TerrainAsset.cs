using System.Runtime.CompilerServices;
using Hexecs.Benchmarks.Map.Terrains.ValueTypes;

namespace Hexecs.Benchmarks.Map.Terrains.Assets;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct TerrainAsset(string name, TerrainType type) : IAssetComponent
{
    public const string Ground = "Base";
    public const string River = "River";
    public const string UrbanConcrete = "UrbanConcrete";

    public readonly string Name = name;
    public readonly TerrainType Type = type;
}