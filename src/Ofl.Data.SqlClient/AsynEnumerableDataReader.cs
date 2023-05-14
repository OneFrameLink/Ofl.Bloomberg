using System.Collections;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Ofl.Data.SqlClient;

// TODO: Consider extending from SqlDataReader so that
// bytes can be streamed directly to the DB for strings
// instead of allocating strings entirely (if there is access
// to bytes, that is).
public class AsynEnumerableDataReader<T> : DbDataReader
    where T : class
{
    #region Instance, read-only state

    private readonly IAsyncEnumerator<T> _enumerator;

    private readonly ISqlBulkCopyRowMapper<T> _mapper;

    #endregion

    #region Constructor

    public AsynEnumerableDataReader(
        IAsyncEnumerable<T> enumerable
        , ISqlBulkCopyRowMapper<T> mapper
    )
    {
        // Assign values.
        _enumerator = enumerable.GetAsyncEnumerator();
        _mapper = mapper;
    }

    #endregion

    #region DbDataReader overrides

    public override object GetValue(int ordinal)
    {
        // Current implementation of SqlBulkCopy indicates that this is called
        // once per value per row.  Because of that, we are *not* caching
        // the values, and passing through.
        // If that changes, and this is called multiple times, then we will cache
        // the values.
        // NOTE: GetValue should have a return type of object?, which SqlBulkWriter
        // will handle correctly.
        return _mapper.Map(_enumerator.Current, ordinal)!;
    }

    public override int FieldCount => _mapper.FieldCount;

    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        // Move to the next item.
        var task = _enumerator.MoveNextAsync();

        // If completed, return the result otherwise, complete a task and return
        // that result.
        return task.IsCompleted
            // Note, caching for bool results is currently implemented
            // and not likely to change, as per:
            // https://github.com/dotnet/runtime/blob/81977309048600e67fdb44a7d4c99aaad89846d7/src/libraries/System.Private.CoreLib/src/System/Threading/Tasks/Task.cs#L5222-L5273
            // and
            // https://devblogs.microsoft.com/dotnet/how-async-await-really-works/#:~:text=And%20in%20fact%2C%20Task.FromResult%20does%20that%20today
            ? Task.FromResult(task.Result)
            // Convert to a task and return that with a continuation.
            // Unfortunately, this results in an allocation but returns
            // what we're looking for.
            : task.AsTask();
    }

    #endregion

    #region Disposable implementations

    public override async ValueTask DisposeAsync()
    {
        // Call the base and then the other items.
        await base.DisposeAsync();
        await _enumerator.DisposeAsync().ConfigureAwait(false);

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposing)
    {
        // Call the base.
        base.Dispose(disposing);

        // If disposing, dispose of the mapper.
        if (disposing)
        {
            // Dispose.
            using var _ = _mapper;
        }
    }

    #endregion

    #region Not implemented DbDataReader overrides

    public override object this[int ordinal] => throw new NotImplementedException();

    public override object this[string name] => throw new NotImplementedException();

    public override int Depth => throw new NotImplementedException();

    public override bool HasRows => throw new NotImplementedException();

    public override bool IsClosed => throw new NotImplementedException();

    public override int RecordsAffected => throw new NotImplementedException();

    public override bool GetBoolean(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override byte GetByte(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override decimal GetDecimal(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override double GetDouble(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public override Type GetFieldType(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override float GetFloat(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override Guid GetGuid(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override short GetInt16(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetInt32(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetInt64(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override string GetName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetOrdinal(string name)
    {
        throw new NotImplementedException();
    }

    public override string GetString(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public override bool IsDBNull(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override bool NextResult()
    {
        throw new NotImplementedException();
    }

    public override bool Read()
    {
        throw new NotImplementedException();
    }

    #endregion
}
