using System.Buffers;

namespace Ofl.Bloomberg.BackOffice;

public static partial class BloombergBackOfficeReaderExtensions
{
    #region Extensions

    public static async Task AccumulateSectionAsync(
        this BloombergBackOfficeReader bloombergBackOfficeReader
        , ReadOnlyMemory<byte>? startLine
        , ReadOnlyMemory<byte> endLine
        , bool advanceBufferAfterEndLine
        , LineAccumulator accumulator
        , CancellationToken cancellationToken = default
    )
    {
        // The line.
        ReadOnlySequence<byte> line;

        // If there is a startline, then read a line and check it
        if (startLine is not null)
        {
            // Read the next line.
            line = await bloombergBackOfficeReader
                .ReadLineAsync(cancellationToken)
                .ConfigureAwait(false);

            // Check it.
            line.EnsureSequenceEqual(startLine.Value.Span);
        }

        // If we do not want to advance, then pass the end line.
        if (advanceBufferAfterEndLine)
            // Read lines normally.
            // While lines are not null.
            while (true)
            {
                // Get the line.
                line = await bloombergBackOfficeReader
                    .ReadLineAsync(cancellationToken)
                    .ConfigureAwait(false);

                // If equal to the end, then bail.
                if (line.SequenceEqual(endLine.Span))
                    return;

                // The caller is responsible for empty/comments, etc.
                // Perform the operation.
                accumulator(line);
            }
        else
        {
            // A nullable line to read.
            ReadOnlySequence<byte>? nullableLine;

            // While lines are not null.
            while ((
                nullableLine = await bloombergBackOfficeReader
                    .ReadLineAsync(endLine, cancellationToken)
                    .ConfigureAwait(false)
            ) is not null)
            {
                // Get the line.
                line = nullableLine.Value;

                // The caller is responsible for empty/comments, etc.
                // Perform the operation.
                accumulator(line);
            }
        }
    }

    #endregion
}
