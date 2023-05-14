using System.Buffers;

namespace Ofl.Bloomberg.BackOffice;

// Based on Bloomberg documentation, which referss to it as "DescriptiveDataFile"
// https://data.bloomberg.com/docs/data-license/#bulk-file-types-DescriptiveDataFile
public sealed class BloombergBackOfficeDescriptiveDataFileRow :
    IReusableBloombergBackOfficeRow<BloombergBackOfficeDescriptiveDataFileRow>
{
    #region Instance state.

    public ReadOnlySequence<byte> Line { get; private set; }

    public ReadOnlySequence<byte> SecurityDescription { get; private set; }

    public int ReturnCode { get; private set; }

    public int NumberOfFields { get; private set; }

    // The backing for the Values property, makes for working with easier.
    private ReadOnlySequence<byte>[] _valuesBacking = Array.Empty<ReadOnlySequence<byte>>();

    // NOTE: We'd *really* like to wrap this in a ReadOnlyCollection
    // so this can't be cast away and direct array access is exposed.
    // But that is an allocation, and there are a *lot* of
    // these rows...
    // So, 🦶 meet 🔫
    public IReadOnlyList<ReadOnlySequence<byte>> Values => _valuesBacking;

    private readonly ReusableBloombergBackOfficeRowParameters _parameters;

    #endregion

    #region Constructor

    public BloombergBackOfficeDescriptiveDataFileRow(
        in ReusableBloombergBackOfficeRowParameters parameters
    )
    {
        // Assign values.
        _parameters = parameters;
    }

    public static BloombergBackOfficeDescriptiveDataFileRow Create(
        in ReusableBloombergBackOfficeRowParameters parameters
    ) => new(parameters);

    public static void Reuse(
        in BloombergBackOfficeDescriptiveDataFileRow instance
        , in ReadOnlySequence<byte> line
    )
    {
        // Set the line on the instance.
        instance.Line = line;

        // Create a sequence reader.
        var reader = new SequenceReader<byte>(instance.Line);

        // Get the id sequence.
        instance.SecurityDescription = reader.ReadNextBloombergFieldValue();

        // Get the action and field count.
        instance.ReturnCode = reader.ReadNextBloombergFieldValueAsIntegerInt32();

        // Store the old number of fields.
        var oldNumberOfFields = instance.NumberOfFields;

        // Set the new value.
        instance.NumberOfFields = reader.ReadNextBloombergFieldValueAsIntegerInt32();

        // Set the values to the old values.
        var values = instance._valuesBacking;

        // If the number of values more than the last one, release
        // the old array and rent a new one.
        if (instance.NumberOfFields > oldNumberOfFields)
        { 
            // Release the old array.
            instance._parameters.ByteArrayPool.Return(values);

            // Rent a new array.
            values = instance._parameters.ByteArrayPool.Rent(instance.NumberOfFields);
        }

        // Cycle through the values and populate.
        for (int i = 0; i < instance.NumberOfFields; i++)
            // Set the item in the values.
            values[i] = reader.ReadNextBloombergFieldValue();

        // Set the fields.
        instance._valuesBacking = values;
    }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        // Free the backing array.
        _parameters.ByteArrayPool.Return(_valuesBacking);
    }

    #endregion
}
