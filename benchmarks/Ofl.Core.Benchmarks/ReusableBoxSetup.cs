namespace Ofl.Core.Benchmarks;

internal class ReusableBoxSetup<T> : IDisposable
    where T : struct
{
    #region Instance state

    public T TestValue;

    public readonly ReusableBox<T>[] ReusableBoxes;

    public readonly object[] Row;

    private bool disposedValue;

    #endregion

    #region Constructor

    public ReusableBoxSetup(int columns) : this(columns, default)
    { }

    public ReusableBoxSetup(int columns, T testValue)
    {
        // Assign values.
        TestValue = testValue;
        ReusableBoxes = new ReusableBox<T>[columns];
        Row = new object[columns];

        // Create the boxes.
        for (int i = 0; i < columns; i++)
            ReusableBoxes[i] = new ReusableBox<T>();
    }

    #endregion

    #region IDisposable implementation.

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
                foreach (var box in ReusableBoxes)
                    box.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ReusableBoxSetup()
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
