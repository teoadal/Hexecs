namespace Hexecs.Worlds;

internal static class WorldDebug
{
    public static World? World
    {
        get;
        set
        {
            if (field != null && field != value) Error.Raise("debug world already set");
            field = value;
        }
    }
}