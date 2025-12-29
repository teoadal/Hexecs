using System.Text.Json;
using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Collections;

namespace Hexecs.Serializations;

public static class JsonWriterExtensions
{
    extension(Utf8JsonWriter writer)
    {
        public Utf8JsonWriter WriteProperty(string propertyName,
            Action<Utf8JsonWriter> value)
        {
            writer.WritePropertyName(propertyName);
            value(writer);
            return writer;
        }

        public Utf8JsonWriter WriteProperty<TArray, TElement>(string propertyName,
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
        public Utf8JsonWriter WriteProperty(string propertyName, ActorId value)
        {
            writer.WriteNumber(propertyName, value.Value);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty<T>(string propertyName, ActorId<T> value)
            where T : struct, IActorComponent
        {
            writer.WriteNumber(propertyName, value.Value);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty(string propertyName, AssetId value)
        {
            writer.WriteNumber(propertyName, value.Value);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty<T>(string propertyName, AssetId<T> value)
            where T : struct, IAssetComponent
        {
            writer.WriteNumber(propertyName, value.Value);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty(string propertyName, bool value)
        {
            writer.WriteBoolean(propertyName, value);
            return writer;
        }

        [SkipLocalsInit]
        public Utf8JsonWriter WriteProperty(string propertyName, DateOnly value)
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
        public Utf8JsonWriter WriteProperty(string propertyName, int value)
        {
            writer.WriteNumber(propertyName, value);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty(string propertyName, long value)
        {
            writer.WriteNumber(propertyName, value);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty(string propertyName, string value)
        {
            writer.WriteString(propertyName, value);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty(string propertyName, Type value)
        {
            writer.WriteString(propertyName, value.FullName);
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8JsonWriter WriteProperty(string propertyName, uint value)
        {
            writer.WriteNumber(propertyName, value);
            return writer;
        }

        internal Utf8JsonWriter WriteProperty(string propertyName,
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
}