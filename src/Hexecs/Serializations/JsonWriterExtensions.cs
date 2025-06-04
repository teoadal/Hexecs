using System.Text.Json;
using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Collections;

namespace Hexecs.Serializations;

public static class JsonWriterExtensions
{
    public static Utf8JsonWriter WriteProperty(
        this Utf8JsonWriter writer,
        string propertyName,
        Action<Utf8JsonWriter> value)
    {
        writer.WritePropertyName(propertyName);
        value(writer);
        return writer;
    }

    public static Utf8JsonWriter WriteProperty<TArray, TElement>(this Utf8JsonWriter writer,
        string propertyName,
        in TArray array,
        Action<Utf8JsonWriter, TElement> value)
        where TArray: struct, IArray<TElement>
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartArray();

        for (var i = 0; i < array.Length; i++)
        {
            value(writer, array[i]);
        }

        writer.WriteEndArray();

        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, ActorId value)
    {
        writer.WriteNumber(propertyName, value.Value);
        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty<T>(this Utf8JsonWriter writer, string propertyName, ActorId<T> value)
        where T : struct, IActorComponent
    {
        writer.WriteNumber(propertyName, value.Value);
        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, AssetId value)
    {
        writer.WriteNumber(propertyName, value.Value);
        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty<T>(this Utf8JsonWriter writer, string propertyName, AssetId<T> value)
        where T : struct, IAssetComponent
    {
        writer.WriteNumber(propertyName, value.Value);
        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, bool value)
    {
        writer.WriteBoolean(propertyName, value);
        return writer;
    }
    
    [SkipLocalsInit]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, DateOnly value)
    {
        writer.WritePropertyName(propertyName);

        Span<char> buffer = stackalloc char[68];
        if (value.TryFormat(buffer, out var charsWritten))
        {
            writer.WriteStringValue(buffer[..charsWritten]);
        }
        else
        {
            writer.WriteStringValue(string.Empty);
        }

        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, int value)
    {
        writer.WriteNumber(propertyName, value);
        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, long value)
    {
        writer.WriteNumber(propertyName, value);
        return writer;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, string value)
    {
        writer.WriteString(propertyName, value);
        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, Type value)
    {
        writer.WriteString(propertyName, value.FullName);
        return writer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Utf8JsonWriter WriteProperty(this Utf8JsonWriter writer, string propertyName, uint value)
    {
        writer.WriteNumber(propertyName, value);
        return writer;
    }

    internal static Utf8JsonWriter WriteProperty(
        this Utf8JsonWriter writer,
        string propertyName,
        ref readonly InlineBucket<ushort> value)
    {
        writer.WritePropertyName(propertyName);

        writer.WriteStartArray();
        foreach (var element in value)
        {
            writer.WriteNumberValue(element);
        }

        writer.WriteEndArray();

        return writer;
    }
}