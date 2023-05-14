using System.Buffers;

namespace Ofl.Data.SqlClient;

public abstract class SqlBulkCopyRowMapperBase<T> : ISqlBulkCopyRowMapper<T>
{
    #region Instance state

    private bool _disposedValue;

    // NOTE: Internal so that we can grab that in the extensions easier through
    // nameof.
    protected internal readonly IDisposable[]? _disposables;

    private readonly int _boxedFieldCount;

    public int FieldCount { get; }

    #endregion

    #region Constructor

    protected SqlBulkCopyRowMapperBase(
        int fieldCount
        , int boxedFieldCount
    )
    {
        // Assign values.
        FieldCount = fieldCount;
        _boxedFieldCount = boxedFieldCount;

        // If the boxed is greater than 0, set the disposables.
        // Rent an array.
        _disposables = boxedFieldCount > 0
            ? ArrayPool<IDisposable>.Shared.Rent(_boxedFieldCount)
            : null;
    }

    #endregion

    #region Helpers

    protected internal static ArgumentOutOfRangeException 
        CreateInvalidOrdinalArgumentOutOfRangeException(
        int ordinal
    ) => new(
        nameof(ordinal)
        , ordinal
        , $"The {nameof(ordinal)} parameter does not map to a valid column ordinal defined for this mapper."
    );

    #endregion

    #region Overrides

    public abstract object? Map(in T instance, int ordinal);

    #endregion

    #region IDisposable implementation.

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            // The boxed objects are pinned, which is unmanaged
            // and will mess with the GC, so make sure it is
            // called regardless.
            if (_disposables != null)
            {
                // Dispose of what was assigned.
                for (int i = 0; i < _boxedFieldCount; i++)
                    // Dispose the disposable.
                    _disposables[i].Dispose();

                // Release the array.
                ArrayPool<IDisposable>.Shared.Return(_disposables);
            }

            // Do something.
            _disposedValue = true;
        }
    }

    // Since we are pinning stuff and renting arrays
    // we must make sure it is always disposed of as
    // pinning goes into the realm of unmanaged resources.
    ~SqlBulkCopyRowMapperBase() => Dispose(disposing: false);

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
