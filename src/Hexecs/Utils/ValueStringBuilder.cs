using System.Globalization;

namespace Hexecs.Utils;

/// <summary>
/// Copy of https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/Text/ValueStringBuilder.cs
/// </summary>
[DebuggerDisplay("{ToString()}")]
public ref struct ValueStringBuilder
{
    public readonly int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _length;
    }

    private char[]? _array;
    private Span<char> _buffer;
    private int _length;

    #region Constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueStringBuilder(Span<char> buffer)
    {
        _array = null;
        _buffer = buffer;
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueStringBuilder(int capacity)
    {
        _array = ArrayPool<char>.Shared.Rent(capacity);
        _buffer = _array;
        _length = 0;
    }

    #endregion

    #region Append

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(bool value) => Append(value ? bool.TrueString : bool.FalseString);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        var pos = _length;
        if ((uint)pos < (uint)_buffer.Length)
        {
            _buffer[pos] = c;
            _length = pos + 1;
        }
        else
        {
            GrowAndAppend(c);
        }
    }

    [SkipLocalsInit]
    public void Append(int value, ReadOnlySpan<char> format = default, CultureInfo? culture = null)
    {
        Span<char> buffer = stackalloc char[12];
        if (value.TryFormat(buffer, out var written, format, culture))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    [SkipLocalsInit]
    public void Append(DateTime value, ReadOnlySpan<char> format = default, CultureInfo? culture = null)
    {
        Span<char> buffer = stackalloc char[format.Length < 26 ? 26 : format.Length];
        if (value.TryFormat(buffer, out var written, format, culture))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    [SkipLocalsInit]
    public void Append(double value, ReadOnlySpan<char> format = default, CultureInfo? culture = null)
    {
        Span<char> buffer = stackalloc char[36];
        if (value.TryFormat(buffer, out var written, format, culture))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    [SkipLocalsInit]
    public void Append(float value, ReadOnlySpan<char> format = default, CultureInfo? culture = null)
    {
        Span<char> buffer = stackalloc char[36];
        if (value.TryFormat(buffer, out var written, format, culture))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    [SkipLocalsInit]
    public void Append(Guid value, ReadOnlySpan<char> format = default)
    {
        Span<char> buffer = stackalloc char[format.Length < 38 ? 38 : format.Length];
        if (value.TryFormat(buffer, out var written, format))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    [SkipLocalsInit]
    public void Append(uint value, ReadOnlySpan<char> format = default, CultureInfo? culture = null)
    {
        Span<char> buffer = stackalloc char[68];
        if (value.TryFormat(buffer, out var written, format, culture))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    [SkipLocalsInit]
    public void Append(long value, ReadOnlySpan<char> format = default, CultureInfo? culture = null)
    {
        Span<char> buffer = stackalloc char[68];
        if (value.TryFormat(buffer, out var written, format, culture))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s)
    {
        if (string.IsNullOrEmpty(s)) return;

        var pos = _length;
        if (s.Length == 1 && (uint)pos < (uint)_buffer.Length)
        {
            _buffer[pos] = s[0];
            _length = pos + 1;
        }
        else Append(s.AsSpan());
    }

    [SkipLocalsInit]
    public void Append(TimeSpan value, ReadOnlySpan<char> format = default, CultureInfo? culture = null)
    {
        Span<char> buffer = stackalloc char[format.Length < 36 ? 36 : format.Length];
        if (value.TryFormat(buffer, out var written, format, culture))
        {
            Append(buffer[..written]);
        }
        else CantFormatToString(value);
    }

    public void Append(scoped Span<char> value)
    {
        var pos = _length;
        if (pos > _buffer.Length - value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_buffer[_length..]);
        _length += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(in ReadOnlyMemory<char> value) => Append(value.Span);
    
    public void Append(scoped ReadOnlySpan<char> value)
    {
        var pos = _length;
        if (pos > _buffer.Length - value.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_buffer[_length..]);
        _length += value.Length;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<char> AsReadonlySpan() => _buffer[.._length];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AsSpan() => _buffer;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _length = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        var toReturn = _array;
        this = default;
        if (toReturn != null) ArrayPool<char>.Shared.Return(toReturn);
    }

    public void EnsureCapacity(int capacity)
    {
        if ((uint)capacity > (uint)_buffer.Length) Grow(capacity - _length);
    }

    public string Flush()
    {
        var result = _length == 0
            ? string.Empty
            : _buffer[.._length].ToString();

        Dispose();

        return result;
    }
    
    public readonly override string ToString() => _length == 0
        ? string.Empty
        : _buffer[.._length].ToString();

    public void TrimEnd()
    {
        var buffer = _buffer;
        var end = _length - 1;
        for (; end >= 0; end--)
        {
            if (!char.IsWhiteSpace(buffer[end]))
            {
                break;
            }
        }

        _length = end + 1;
    }

    public void Whitespace() => Append(' ');

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        Grow(1);
        Append(c);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        const uint arrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        var newCapacity = (int)Math.Max(
            (uint)(_length + additionalCapacityBeyondPos),
            Math.Min((uint)_buffer.Length * 2, arrayMaxLength));

        var poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

        _buffer[.._length].CopyTo(poolArray);

        var toReturn = _array;
        _buffer = _array = poolArray;
        if (toReturn != null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    [DoesNotReturn]
    private static void CantFormatToString<T>(T value)
    {
        throw new Exception($"Can't format {value} to string");
    }
}