using System.Collections;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ofl.Data.SqlClient;

// TODO: Consider extending from SqlDataReader so that
// bytes can be streamed directly to the DB for strings
// instead of allocating strings entirely (if there is access
// to bytes, that is).
public class AsyncEnumerableDataReader<T> : DbDataReader
    where T : class
{
    #region Instance state

    private readonly IAsyncEnumerator<T> _enumerator;

    private readonly ISqlBulkCopyRowMapper<T> _mapper;

    private T _current = default!;

    #endregion

    #region Constructor

    public AsyncEnumerableDataReader(
        IAsyncEnumerable<T> enumerable
        , ISqlBulkCopyRowMapper<T> mapper
    )
    {
        // Assign values.
        _enumerator = enumerable.GetAsyncEnumerator();
        _mapper = mapper;
    }

    #endregion

    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CacheCurrent(Task<bool> result)
    {
        // Get the value.
        var value = result.Result;

        // If there is a result, set current.
        if (value)
            // The task is completed.
            _current = _enumerator.Current;

        // Return the value.
        return value;
    }

    #endregion

    #region DbDataReader overrides

    public override object GetValue(int ordinal)
    {
        // Current implementation of SqlBulkCopy indicates that this is called
        // once per value per row.  Because of that, we are *not* caching
        // the mapped values, and passing through.
        // If that changes, and this is called multiple times, then we will cache
        // the values.
        // NOTE: GetValue should have a return type of object?, which SqlBulkWriter
        // will handle correctly.
        // Call the mapper.
        return _mapper.Map(in _current, ordinal)!;
    }

    public override int FieldCount => _mapper.FieldCount;

    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        // Move to the next item.
        var task = _enumerator.MoveNextAsync();

        // If this is completed, then set the current once, we do
        // not need to access it multiple times.
        if (task.IsCompleted)
        {
            // Get the result.
            var taskResult = task.Result;

            // Get the return value.
            // Note, caching for bool results is currently implemented
            // and not likely to change, as per:
            // https://github.com/dotnet/runtime/blob/81977309048600e67fdb44a7d4c99aaad89846d7/src/libraries/System.Private.CoreLib/src/System/Threading/Tasks/Task.cs#L5222-L5273
            // and
            // https://devblogs.microsoft.com/dotnet/how-async-await-really-works/#:~:text=And%20in%20fact%2C%20Task.FromResult%20does%20that%20today
            var result = Task.FromResult(taskResult);

            // Cache.
            CacheCurrent(result);

            // Return.
            return result;
        }

        // Return as a task, continue when done on success to cache the current
        // so mappings can be more efficient.
        return task
            .AsTask()
            .ContinueWith(
                // TODO: This seems to allocate, is there a way
                // to prevent?
                CacheCurrent
                , TaskContinuationOptions.OnlyOnRanToCompletion
                | TaskContinuationOptions.ExecuteSynchronously
            );
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
