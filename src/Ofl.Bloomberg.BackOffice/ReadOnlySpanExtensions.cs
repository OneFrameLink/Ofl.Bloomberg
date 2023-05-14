using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public static class ReadOnlySpanExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DecodeString(
        this in ReadOnlySpan<byte> buffer
    ) => buffer.IsEmpty ? string.Empty : Constant.Encoding.GetString(buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Parse<T>(
        this in ReadOnlySpan<byte> span
        , IFormatProvider? formatProvider = default
    )
        where T : ISpanParsable<T>
    {
        // Allocate the characters on the stack.
        Span<char> chars = stackalloc char[Constant.Encoding.GetCharCount(span)];

        // Get the characters.
        Constant.Encoding.GetChars(span, chars);

        // Parse.
        return T.Parse(chars, formatProvider ?? CultureInfo.InvariantCulture);
    }
}
