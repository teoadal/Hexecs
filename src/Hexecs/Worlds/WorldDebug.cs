using Hexecs.Utils;

namespace Hexecs.Worlds;

internal static class WorldDebug
{
    public static World? World
    {
        get => _world;
        set
        {
            if (_world != null && _world != value) Error.Raise("debug world already set");
            _world = value;
        }
    }

    private static World? _world;
}