using System.Buffers;

namespace Ofl.Bloomberg.BackOffice;

public delegate void LineAccumulator(in ReadOnlySequence<byte> line);
