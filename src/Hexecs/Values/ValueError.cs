namespace Hexecs.Values;

internal static class ValueError
{
    [DoesNotReturn]
    public static void TableAlreadyExists(string name)
    {
        throw new Exception($"Table with name '{name}' already exists");
    }

    [DoesNotReturn]
    public static void TableNotFound(string name)
    {
        throw new Exception($"Table with name '{name}' isn't found");
    }

    [DoesNotReturn]
    public static void TableNotExpected(string name, Type actualKeyType)
    {
        throw new Exception($"Table with name '{name}' found, " +
                            $"but type of key is '{TypeOf.GetTypeName(actualKeyType)}'");
    }

    [DoesNotReturn]
    public static void TableNotExpected(string name, Type actualKeyType, Type actualValueType)
    {
        throw new Exception($"Table with name '{name}' found, " +
                            $"but type of key is '{TypeOf.GetTypeName(actualKeyType)}' " +
                            $"and type of value is {TypeOf.GetTypeName(actualValueType)}");
    }
}