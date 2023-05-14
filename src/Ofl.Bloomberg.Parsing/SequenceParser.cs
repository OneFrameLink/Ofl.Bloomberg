using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.Parsing;

public static class SequenceParser
{
    #region Helpers

    /// <inheritdoc cref="SpanParser.ParseDetail(in ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParsingDetail ParseDetail(
        this in ReadOnlySequence<byte> source
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.ParseDetail();

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.ParseDetail();
    }

    #endregion

    #region Date parsing.

    /// <inheritdoc cref="SpanParser.TryParseDate(in ReadOnlySpan{byte}, out DateOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseDate(
        this in ReadOnlySequence<byte> source
        , out DateOnly value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseDate(
                out value
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseDate(
            out value
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseDate(in ReadOnlySpan{byte}, out DateOnly, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseDate(
        this in ReadOnlySequence<byte> source
        , out DateOnly value
        , out ParsingDetail detail
    ) => source.TryParseDate(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseDate(in ReadOnlySpan{byte}, out DateOnly)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseDate(
        this in ReadOnlySequence<byte> source
        , out DateOnly value
    ) => source.TryParseDate(out value, false, out _);

    #endregion

    #region Time parsing.

    /// <inheritdoc cref="SpanParser.TryParseTime(in ReadOnlySpan{byte}, out TimeOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseTime(
        this in ReadOnlySequence<byte> source
        , out TimeOnly value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseTime(
                out value
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseTime(
            out value
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseTime(in ReadOnlySpan{byte}, out TimeOnly, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseTime(
        this in ReadOnlySequence<byte> source
        , out TimeOnly value
        , out ParsingDetail detail
    ) => source.TryParseTime(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseTime(in ReadOnlySpan{byte}, out TimeOnly)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseTime(
        this in ReadOnlySequence<byte> source
        , out TimeOnly value
    ) => source.TryParseTime(out value, false, out _);

    #endregion

    #region Date or time parsing.

    /// <inheritdoc cref="SpanParser.TryParseDateOrTime(in ReadOnlySpan{byte}, out DateOnly, out TimeOnly, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseDateOrTimeResult TryParseDateOrTime(
        this in ReadOnlySequence<byte> source
        , out DateOnly date
        , out TimeOnly time
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseDateOrTime(
                out date
                , out time
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseDateOrTime(
            out date
            , out time
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseDateOrTime(in ReadOnlySpan{byte}, out DateOnly, out TimeOnly, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseDateOrTimeResult TryParseDateOrTime(
        this in ReadOnlySequence<byte> source
        , out DateOnly date
        , out TimeOnly time
        , out ParsingDetail detail
    ) => source.TryParseDateOrTime(out date, out time, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseDateOrTime(in ReadOnlySpan{byte}, out DateOnly, out TimeOnly)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseDateOrTimeResult TryParseDateOrTime(
        this in ReadOnlySequence<byte> source
        , out DateOnly date
        , out TimeOnly time
    ) => source.TryParseDateOrTime(out date, out time, false, out _);

    #endregion

    #region Month/year parsing.

    /// <inheritdoc cref="SpanParser.TryParseMonthYear(in ReadOnlySpan{byte}, out int, out int, out ReadOnlySpan{byte}, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYear(
        this in ReadOnlySequence<byte> source
        , out int month
        , out int year
        , out ReadOnlySpan<byte> period
        , bool parseDetail
        , out ParsingDetail detail
    ) => throw new NotSupportedException("Need to solve the releasing of the period parameter.");

    /// <inheritdoc cref="SpanParser.TryParseMonthYear(in ReadOnlySpan{byte}, out int, out int, out ReadOnlySpan{byte}, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYear(
        this in ReadOnlySequence<byte> source
        , out int month
        , out int year
        , out ReadOnlySpan<byte> period
        , out ParsingDetail detail
    ) => source.TryParseMonthYear(out month, out year, out period, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseMonthYear(in ReadOnlySpan{byte}, out int, out int, out ReadOnlySpan{byte})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMonthYear(
        this in ReadOnlySequence<byte> source
        , out int month
        , out int year
        , out ReadOnlySpan<byte> period
    ) => source.TryParseMonthYear(out month, out year, out period, false, out _);

    #endregion

    #region Boolean parsing

    /// <inheritdoc cref="SpanParser.TryParseBoolean(in ReadOnlySpan{byte}, out bool, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseBoolean(
        this in ReadOnlySequence<byte> source
        , out bool value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseBoolean(
                out value
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseBoolean(
            out value
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseBoolean(in ReadOnlySpan{byte}, out bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseBoolean(
        this in ReadOnlySequence<byte> source
        , out bool value
        , out ParsingDetail detail
    ) => source.TryParseBoolean(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseBoolean(in ReadOnlySpan{byte}, out bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseBoolean(
        this in ReadOnlySequence<byte> source
        , out bool value
    ) => source.TryParseBoolean(out value, false, out _);

    #endregion

    #region Integer parsing

    /// <inheritdoc cref="SpanParser.TryParseIntegerInt32(in ReadOnlySpan{byte}, out int, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt32(
        this in ReadOnlySequence<byte> source
        , out int value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseIntegerInt32(
                out value
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseIntegerInt32(
            out value
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseIntegerInt64(in ReadOnlySpan{byte}, out long, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt64(
        this in ReadOnlySequence<byte> source
        , out long value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseIntegerInt64(
                out value
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseIntegerInt64(
            out value
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseIntegerBigInteger(in ReadOnlySpan{byte}, out BigInteger, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerBigInteger(
        this in ReadOnlySequence<byte> source
        , out BigInteger value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseIntegerBigInteger(
                out value
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseIntegerBigInteger(
            out value
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseIntegerInt32(in ReadOnlySpan{byte}, out int, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt32(
        this in ReadOnlySequence<byte> source
        , out int value
        , out ParsingDetail detail
    ) => source.TryParseIntegerInt32(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseIntegerInt32(in ReadOnlySpan{byte}, out int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt32(
        this in ReadOnlySequence<byte> source
        , out int value
    ) => source.TryParseIntegerInt32(out value, false, out _);

    /// <inheritdoc cref="SpanParser.TryParseIntegerInt64(in ReadOnlySpan{byte}, out long, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt64(
        this in ReadOnlySequence<byte> source
        , out long value
        , out ParsingDetail detail
    ) => source.TryParseIntegerInt64(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseIntegerInt64(in ReadOnlySpan{byte}, out long)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerInt64(
        this in ReadOnlySequence<byte> source
        , out long value
    ) => source.TryParseIntegerInt64(out value, false, out _);

    /// <inheritdoc cref="SpanParser.TryParseIntegerBigInteger(in ReadOnlySpan{byte}, out BigInteger, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerBigInteger(
        this in ReadOnlySequence<byte> source
        , out BigInteger value
        , out ParsingDetail detail
    ) => source.TryParseIntegerBigInteger(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseIntegerBigInteger(in ReadOnlySpan{byte}, out BigInteger)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseIntegerBigInteger(
        this in ReadOnlySequence<byte> source
        , out BigInteger value
    ) => source.TryParseIntegerBigInteger(out value, false, out _);

    #endregion

    #region Real parsing.

    /// <inheritdoc cref="SpanParser.TryParseReal(in ReadOnlySpan{byte}, out decimal, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseReal(
        this in ReadOnlySequence<byte> source
        , out decimal value
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseReal(
                out value
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseReal(
            out value
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseReal(in ReadOnlySpan{byte}, out decimal, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseReal(
        this in ReadOnlySequence<byte> source
        , out decimal value
        , out ParsingDetail detail
    ) => source.TryParseReal(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParseReal(in ReadOnlySpan{byte}, out decimal)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseReal(
        this in ReadOnlySequence<byte> source
        , out decimal value
    ) => source.TryParseReal(out value, false, out _);

    #endregion

    #region Price parsing.

    /// <inheritdoc cref="SpanParser.TryParsePrice(in ReadOnlySpan{byte}, out decimal, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParsePrice(
        this in ReadOnlySequence<byte> source
        , out decimal value
        , out ParsingDetail detail
    ) => source.TryParseReal(out value, true, out detail);

    /// <inheritdoc cref="SpanParser.TryParsePrice(in ReadOnlySpan{byte}, out decimal)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParsePrice(
        this in ReadOnlySequence<byte> source
        , out decimal value
    ) => source.TryParseReal(out value, false, out _);

    #endregion

    #region Integer/real parsing.

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealInt32(in ReadOnlySpan{byte}, out int, out decimal, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt32(
        this in ReadOnlySequence<byte> source
        , out int integer
        , out decimal real
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseIntegerRealInt32(
                out integer
                , out real
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int) source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseIntegerRealInt32(
            out integer
            , out real
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealInt64(in ReadOnlySpan{byte}, out long, out decimal, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt64(
        this in ReadOnlySequence<byte> source
        , out long integer
        , out decimal real
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseIntegerRealInt64(
                out integer
                , out real
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseIntegerRealInt64(
            out integer
            , out real
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealBigInteger(in ReadOnlySpan{byte}, out BigInteger, out decimal, bool, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealBigInteger(
        this in ReadOnlySequence<byte> source
        , out BigInteger integer
        , out decimal real
        , bool parseDetail
        , out ParsingDetail detail
    )
    {
        // If there is one sequence, then use that.
        if (source.IsSingleSegment)
            return source.FirstSpan.TryParseIntegerRealBigInteger(
                out integer
                , out real
                , parseDetail
                , out detail
            );

        // Copy to a local span.
        Span<byte> copied = stackalloc byte[(int)source.Length];

        // Copy to the span.
        source.CopyTo(copied);

        // Read only.
        ReadOnlySpan<byte> ro = copied;

        // Parse.
        return ro.TryParseIntegerRealBigInteger(
            out integer
            , out real
            , parseDetail
            , out detail
        );
    }

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealInt32(in ReadOnlySpan{byte}, out int, out decimal, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt32(
        this in ReadOnlySequence<byte> source
        , out int integer
        , out decimal real
        , out ParsingDetail detail
    ) => source.TryParseIntegerRealInt32(
        out integer
        , out real
        , true
        , out detail
    );

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealInt32(in ReadOnlySpan{byte}, out int, out decimal)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt32(
        this in ReadOnlySequence<byte> source
        , out int integer
        , out decimal real
    ) => source.TryParseIntegerRealInt32(
        out integer
        , out real
        , false
        , out _
    );

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealInt64(in ReadOnlySpan{byte}, out long, out decimal, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt64(
        this in ReadOnlySequence<byte> source
        , out long integer
        , out decimal real
        , out ParsingDetail detail
    ) => source.TryParseIntegerRealInt64(
        out integer
        , out real
        , true
        , out detail
    );

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealInt64(in ReadOnlySpan{byte}, out long, out decimal)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealInt64(
        this in ReadOnlySequence<byte> source
        , out long integer
        , out decimal real
    ) => source.TryParseIntegerRealInt64(
        out integer
        , out real
        , false
        , out _
    );

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealBigInteger(in ReadOnlySpan{byte}, out BigInteger, out decimal, out ParsingDetail)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealBigInteger(
        this in ReadOnlySequence<byte> source
        , out BigInteger integer
        , out decimal real
        , out ParsingDetail detail
    ) => source.TryParseIntegerRealBigInteger(
        out integer
        , out real
        , true
        , out detail
    );

    /// <inheritdoc cref="SpanParser.TryParseIntegerRealBigInteger(in ReadOnlySpan{byte}, out BigInteger, out decimal)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ParseIntegerRealResult TryParseIntegerRealBigInteger(
        this in ReadOnlySequence<byte> source
        , out BigInteger integer
        , out decimal real
    ) => source.TryParseIntegerRealBigInteger(
        out integer
        , out real
        , false
        , out _
    );

    #endregion
}
