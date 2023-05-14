using System.Buffers;

namespace Ofl.Bloomberg.BackOffice;

public delegate void KeyValueAccumulator(
    in ReadOnlySequence<byte> key
    , in ReadOnlySequence<byte> value
);
