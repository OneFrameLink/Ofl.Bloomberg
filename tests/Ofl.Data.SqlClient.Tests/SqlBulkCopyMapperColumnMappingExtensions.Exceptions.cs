namespace Ofl.Data.SqlClient.Tests;

public partial class SqlBulkCopyMapperColumnMappingExtensions
{
    #region Tests

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_With_Null_ColumnName_Fails()
    {
        // Create the "bad" mapping.
        var mappings = new SqlBulkCopyMapperColumnMapping[] {
            SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(null!, 0)
                , (in ReferenceTypeInput x) => x.IntValue
            )
        }
        .AsReadOnly();

        // Assert
        Assert.Throws<ArgumentException>(
            () => SqlClient.SqlBulkCopyMapperColumnMappingExtensions
                .CreateSqlBulkCopyMapper<ReferenceTypeInput>(mappings)
        );
    }

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_With_Null_ColumnOrdinal_Fails()
    {
        // Create the "bad" mapping.
        var mappings = new SqlBulkCopyMapperColumnMapping[] {
            SqlBulkCopyMapperColumnMapping.FromDelegate(
                new NullableColumnOrdinalDbColumn(nameof(ReferenceTypeInput.IntValue))
                , (in ReferenceTypeInput x) => x.IntValue
            )
        }
        .AsReadOnly();

        // Assert
        Assert.Throws<ArgumentException>(
            () => SqlClient.SqlBulkCopyMapperColumnMappingExtensions
                .CreateSqlBulkCopyMapper<ReferenceTypeInput>(mappings)
        );
    }

    [Fact]
    public void Test_CreateSqlBulkCopyMapper_With_Multiple_Same_ColumnOrdinals_Fails()
    {
        // Create the "bad" mapping.
        var mappings = new SqlBulkCopyMapperColumnMapping[] {
            SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(nameof(ReferenceTypeInput.IntValue), 0)
                , (in ReferenceTypeInput x) => x.IntValue
            )
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(nameof(ReferenceTypeInput.NullableIntValue), 0)
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
    public void Test_CreateSqlBulkCopyMapper_With_Multiple_Same_ColumnNames_Fails()
    {
        // Create the "bad" mapping.
        var mappings = new SqlBulkCopyMapperColumnMapping[] {
            SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(nameof(ReferenceTypeInput.IntValue), 0)
                , (in ReferenceTypeInput x) => x.IntValue
            )
            , SqlBulkCopyMapperColumnMapping.FromDelegate(
                new TestDbColumn(nameof(ReferenceTypeInput.IntValue), 1)
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