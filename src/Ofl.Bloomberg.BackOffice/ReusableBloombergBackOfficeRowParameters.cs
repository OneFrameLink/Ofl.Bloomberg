using System.Buffers;

namespace Ofl.Bloomberg.BackOffice;

public record struct ReusableBloombergBackOfficeRowParameters(
    ArrayPool<ReadOnlySequence<byte>> ByteArrayPool
);
