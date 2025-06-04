namespace Hexecs.Utils;

internal static class TypeOf
{
    public static int NextId;

    public static void BuildTypeName(Type type, ref ValueStringBuilder sb)
    {
        if (!type.IsGenericType)
        {
            var simpleName = type.Name;
            sb.Append(simpleName);
            return;
        }

        var genericArguments = type.GetGenericArguments();

        sb.Append(type
            .GetGenericTypeDefinition().Name
            .Replace($"`{genericArguments.Length}", string.Empty));

        sb.Append('<');

        var first = true;
        foreach (var genericArgument in genericArguments)
        {
            if (first) first = false;
            else sb.Append(", ");

            BuildTypeName(genericArgument, ref sb);
        }

        sb.Append('>');
    }

    [SkipLocalsInit]
    public static string GetTypeName(Type type)
    {
        if (!type.IsGenericType) return type.Name;

        var sb = new ValueStringBuilder(stackalloc char[128]);
        BuildTypeName(type, ref sb);
        return sb.Flush();
    }
}

// ReSharper disable once UnusedTypeParameter
internal static class TypeOf<T>
{
    // ReSharper disable once StaticMemberInGenericType
    public static readonly int Id = Interlocked.Increment(ref TypeOf.NextId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetTypeName() => TypeOf.GetTypeName(typeof(T));
}