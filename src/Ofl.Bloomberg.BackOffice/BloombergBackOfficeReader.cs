using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.BackOffice;

public class BloombergBackOfficeReader : IDisposable
{
    #region Instance state

    private bool disposedValue;

    private readonly Stream _stream;

    private readonly PipeReader _reader;

    private ReadOnlySequence<byte>? _nullableBuffer;

    private ReadResult _lastReadResult;

    #endregion

    #region Constructor

    public BloombergBackOfficeReader(
        // TODO: Consider other options to configure the pipe.
        Stream stream        
    )
    {
        // Assign values.
        _stream = stream;

        // Create the pipe.
        _reader = PipeReader.Create(
            stream
        );
    }

    #endregion

    #region Helpers

    private static InvalidOperationException CreateEndOfDataStreamReachedException() =>
        new("End of data stream reached unexpectedly.");


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStoredBuffer(in ReadOnlySequence<byte> buffer)
        => _nullableBuffer = buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySequence<byte>? TryReadToDelimiter(
        ref ReadOnlySequence<byte> buffer
        , in byte delimiter
    )
    {
        // The sequence.
        ReadOnlySequence<byte>? nullableSequence;

        // Try and read the line.
        nullableSequence = buffer.TryReadToDelimiter(delimiter);

        // If there is a sequence, return that.
        if (nullableSequence is not null)
            // Set the buffer.
            SetStoredBuffer(buffer);

        // Return the sequence.
        return nullableSequence;
    }

    #endregion

    #region Actions

    public async ValueTask<ReadOnlySequence<byte>> ReadToDelimiterAsync(
        byte delimiter
        , CancellationToken cancellationToken = default
    )
    {
        // The sequence.
        ReadOnlySequence<byte>? nullableSequence;

        // The buffer.
        ReadOnlySequence<byte> buffer;

        // If there is a buffer, then try and to the delimiter.
        if (_nullableBuffer is not null && !_nullableBuffer.Value.IsEmpty)
        {
            // Get the buffer.
            buffer = _nullableBuffer.Value;

            // Try to read to the delimiter.
            nullableSequence = TryReadToDelimiter(ref buffer, delimiter);

            // If there is a sequence, return that.
            if (nullableSequence is not null)
                // Return the sequence.
                return nullableSequence.Value;

            // There's nothing that can be read,
            // move on to reading more from the reader.
        }

        // Read.
        while (true)
        {
            // If there is nothing left to read, throw.
            if (_lastReadResult.IsCompleted)
                // Throw.
                throw CreateEndOfDataStreamReachedException();

            // If there is a buffer, the reader needs to be advanced
            if (_nullableBuffer is not null)
            {
                // Get the buffer.
                buffer = _nullableBuffer.Value;

                // Tell the PipeReader how much of the buffer we have consumed (if
                // at all).
                _reader.AdvanceTo(buffer.Start, buffer.End);
            }

            // Get the result.
            _lastReadResult = await _reader
                .ReadAsync(cancellationToken)
                .ConfigureAwait(false);

            // Get the buffer.
            buffer = _lastReadResult.Buffer;

            // Try and read to the delimiter.
            nullableSequence = TryReadToDelimiter(ref buffer, delimiter);

            // If there is a sequence
            if (nullableSequence is not null)
                // Return the value.
                return nullableSequence.Value;
        }
    }

    public async ValueTask<ReadOnlySequence<byte>?> ReadLineAsync(
        ReadOnlyMemory<byte> doNotAdvanceIfLineEquals
        , CancellationToken cancellationToken = default
    )
    {
        // Tries to read a line, only advancing if the line does not
        // equal what is passed in.
        ReadOnlySequence<byte>? TryToReadLine(
            ref ReadOnlySequence<byte> buffer
            , out bool read
        ) {
            // Assume success.
            read = true;

            // The sequence.
            ReadOnlySequence<byte>? nullableSequence;

            // Store the old buffer.
            var oldBuffer = buffer;

            // Try and read the line.
            nullableSequence = buffer.TryReadToDelimiter(
                Constant.NewLine
            );

            // If the sequence is null, set read to false
            // and bail.
            if (nullableSequence is null)
            {
                // Set read to false.
                read = false;

                // Return null.
                return null;
            }

            // Get the sequence
            var sequence = nullableSequence.Value;

            // Check to see if it is equal to the memory passed in.
            if (sequence.SequenceEqual(doNotAdvanceIfLineEquals.Span))
            {
                // Set the buffer to the old buffer.
                buffer = oldBuffer;

                // Return null.
                return null;
            }

            // Set the stored buffer.
            SetStoredBuffer(buffer);

            // Return the sequence.
            return sequence;
        }

        // The buffer.
        ReadOnlySequence<byte> buffer;

        // If there is a buffer, then try to read.
        if (_nullableBuffer is not null && !_nullableBuffer.Value.IsEmpty)
        {
            // Get the buffer.
            buffer = _nullableBuffer.Value;

            // Try to read the line.
            var nullableLine = TryToReadLine(ref buffer, out var read);

            // If this was read, return the value.
            if (read) return nullableLine;

            // This was not read, not much more to do.
            // Move on to reading more from the reader.
        }

        // Read.
        while (true)
        {
            // If at the end, throw.
            if (_lastReadResult.IsCompleted)
                throw CreateEndOfDataStreamReachedException();

            // If there is a buffer, the reader needs to be advanced
            if (_nullableBuffer is not null)
            {
                // Get the buffer.
                buffer = _nullableBuffer.Value;

                // Tell the PipeReader how much of the buffer we have consumed (if
                // at all).
                _reader.AdvanceTo(buffer.Start, buffer.End);
            }

            // Get the result.
            _lastReadResult = await _reader
                .ReadAsync(cancellationToken)
                .ConfigureAwait(false);

            // Get the buffer.
            buffer = _lastReadResult.Buffer;

            // Try and read to the delimiter.
            var nullableLine = TryToReadLine(ref buffer, out var read);

            // If something was read, then return it.
            if (read) return nullableLine;

            // Tell the PipeReader how much of the buffer we have consumed (if
            // at all).
            _reader.AdvanceTo(buffer.Start, buffer.End);
        }
    }

    #endregion

    #region IDisposable implementation

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Complete the reading, no more, frees up
                // memory in the pipe as well.
                _reader.Complete();

                // dispose managed state (managed objects)
                using var _ = _stream;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~BloombergBackOfficeReader()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}