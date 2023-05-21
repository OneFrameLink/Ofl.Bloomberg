namespace Ofl.Data.SqlClient.Tests;

public partial class SqlBulkCopyMapperColumnMappingExtensions
{
    #region Tests

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_With_Multiple_Same_ColumnOrdinals_Fails()
    {
        // Create the "bad" mapping.
        var mappings = new SqlBulkCopyMapperColumnMapping[] {
            SqlBulkCopyMapperColumnMapping.FromDelegate(
                0
                , (in ReferenceTypeInput x) => x.Int32Value
            )
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                0
                , (in ReferenceTypeInput x) => x.NullableIntValue
            )
        }
        .AsReadOnly();

        // Assert.
        Assert.Throws<ArgumentException>(
            () => SqlClient.SqlBulkCopyMapperColumnMappingExtensions
                .CreateSqlBulkCopyMapper<ReferenceTypeInput>(mappings)
        );
    }

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_With_Zero_Based_Packed_Indices_Accessing_Negative_Index_Fails()
    {
        // Create the mappings.
        var mappings = CreateInputMappings();

        // Create an instance, doesn't matter.
        var instance = new ReferenceTypeInput();

        // Call the builder to create the mapper
        using var mapper = SqlClient.SqlBulkCopyMapperColumnMappingExtensions
            .CreateSqlBulkCopyMapper<ReferenceTypeInput>(mappings);

        // Assert.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => mapper.Map(instance, -1)
        );
    }

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_With_Zero_Based_Packed_Indices_Accessing_Index_Plus_One_Fails()
    {
        // Create the mappings.
        var mappings = CreateInputMappings();

        // Create an instance, doesn't matter.
        var instance = new ReferenceTypeInput();

        // Call the builder to create the mapper
        using var mapper = SqlClient.SqlBulkCopyMapperColumnMappingExtensions
            .CreateSqlBulkCopyMapper<ReferenceTypeInput>(mappings);

        // Assert.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => mapper.Map(instance, mapper.FieldCount)
        );
    }

    #endregion
}