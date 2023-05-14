using System.Buffers;

namespace Ofl.Bloomberg.BackOffice;

public interface IReusableBloombergBackOfficeRow<T> : IDisposable
    where T : class
{
    static abstract T Create(
        in ReusableBloombergBackOfficeRowParameters parameters
    );

    static abstract void Reuse(
        in T instance
        , in ReadOnlySequence<byte> line
    );
}
