using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ofl.Core;

// NOTE: This will only work for purely blittable types.
// Things such as BigInteger will have a problem because they
// store references and not everything is blittable.
public class ReusableBox<T> : IDisposable
    where T : struct
{
    #region Instance state

    private readonly object _value;
    
    private bool _disposedValue;

    private readonly GCHandle _handle;

    private readonly unsafe void* _ptr;

    #endregion

    #region Constructor

    public ReusableBox()
    {
        // Get a boxed value.
        // This sets
        _value = new T();

        // Pin the value.        
        _handle = GCHandle.Alloc(_value, GCHandleType.Pinned);

        // Get the pointer.
        unsafe
        {
            _ptr = _handle.AddrOfPinnedObject().ToPointer();
        }
    }

    #endregion

    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe object GetBox(ref T value)
    {
        // Copy the value of T to the pointer.
        Unsafe.Copy(_ptr, ref value);

        // Return.
        return _value;
    }

    #endregion

    #region IDisposable implementation

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // Unpin the handle.
                _handle.Free();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ReusableBox()
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
