using SqRaftEngine.Tql;
using SqRaftEngine.Tql.Abstractions;

namespace SqRaft.UnitTest;

public class TqlParserUnitTest
{
    private readonly ITqlParser _tqlParser = new TqlParser();
    [Theory]
    [InlineData("Person")]
    [InlineData("Person()")]
    public void ShouldGenerateSimpleAsExpected(string tql)
    {
        var table = _tqlParser.ParseTqlLine(tql);

        Assert.Equal("Person", table.Name);
        Assert.Single(table.Columns);
        Assert.Equal("Id", table.Columns[0].Name);
        Assert.Equal("INTEGER", table.Columns[0].Type);
        Assert.True(table.Columns[0].IsPrimaryKey);
        Assert.False(table.Columns[0].Nullable);
        Assert.False(table.Columns[0].Unique);
    }

    [Theory]
    [InlineData("Person(Name)")]
    [InlineData("Person(Id, Name)")]
    public void ShouldGenerateWithDefaultPkAndTextColumn(string tql)
    {
        var table = _tqlParser.ParseTqlLine(tql);

        Assert.Equal("Person", table.Name);
        Assert.Equal(2, table.Columns.Count);
        Assert.Equal("Id", table.Columns[0].Name);
        Assert.Equal("INTEGER", table.Columns[0].Type);
        Assert.True(table.Columns[0].IsPrimaryKey);
        Assert.False(table.Columns[0].Nullable);
        Assert.False(table.Columns[0].Unique);
        Assert.Equal("Name", table.Columns[1].Name);
        Assert.Equal("TEXT", table.Columns[1].Type);
        Assert.False(table.Columns[1].IsPrimaryKey);
        Assert.False(table.Columns[1].Nullable);
        Assert.False(table.Columns[1].Unique);
    }

    [Theory]
    [InlineData("Person(IdNr*+,Name)")]
    [InlineData("Person(IdNr+*,Name)")]
    public void ShouldGenerateWithNonDefaultIntPkAndTextColumn(string tql)
    {
        var table = _tqlParser.ParseTqlLine(tql);

        Assert.Equal("Person", table.Name);
        Assert.Equal(2, table.Columns.Count);
        Assert.Equal("IdNr", table.Columns[0].Name);
        Assert.Equal("INTEGER", table.Columns[0].Type);
        Assert.True(table.Columns[0].IsPrimaryKey);
        Assert.False(table.Columns[0].Nullable);
        Assert.False(table.Columns[0].Unique);
        Assert.Equal("Name", table.Columns[1].Name);
        Assert.Equal("TEXT", table.Columns[1].Type);
        Assert.False(table.Columns[1].IsPrimaryKey);
        Assert.False(table.Columns[1].Nullable);
        Assert.False(table.Columns[1].Unique);

        Assert.DoesNotContain(table.Columns, c => c.Name == "Id"); // no default Id
    }

    [Theory]
    [InlineData("Person(IdNr*,Name)")]
    [InlineData("Person(IdNr*-,Name)")]
    [InlineData("Person(IdNr-*,Name)")]
    public void ShouldGenerateWithNonDefaultTextPkAndTextColumn(string tql)
    {
        var table = _tqlParser.ParseTqlLine(tql);

        Assert.Equal("Person", table.Name);
        Assert.Equal(2, table.Columns.Count);
        Assert.Equal("IdNr", table.Columns[0].Name);
        Assert.Equal("TEXT", table.Columns[0].Type);
        Assert.True(table.Columns[0].IsPrimaryKey);
        Assert.False(table.Columns[0].Nullable);
        Assert.False(table.Columns[0].Unique);
        Assert.Equal("Name", table.Columns[1].Name);
        Assert.Equal("TEXT", table.Columns[1].Type);
        Assert.False(table.Columns[1].IsPrimaryKey);
        Assert.False(table.Columns[1].Nullable);
        Assert.False(table.Columns[1].Unique);

        Assert.DoesNotContain(table.Columns, c => c.Name == "Id"); // no default Id
    }

    [Theory]
    [InlineData("Person(Name!,Age+?)")]
    [InlineData("Person(Name!,Age?+)")]
    public void ShouldGenerateUniqueColumnAndNullableColumn(string tql)
    {
        var table = _tqlParser.ParseTqlLine(tql);

        Assert.Equal("Person", table.Name);
        Assert.Equal(3, table.Columns.Count);
        Assert.Equal("Id", table.Columns[0].Name);
        Assert.Equal("INTEGER", table.Columns[0].Type);
        Assert.True(table.Columns[0].IsPrimaryKey);
        Assert.False(table.Columns[0].Nullable);
        Assert.False(table.Columns[0].Unique);
        Assert.Equal("Name", table.Columns[1].Name);
        Assert.Equal("TEXT", table.Columns[1].Type);
        Assert.False(table.Columns[1].IsPrimaryKey);
        Assert.False(table.Columns[1].Nullable);
        Assert.True(table.Columns[1].Unique);
        Assert.Equal("Age", table.Columns[2].Name);
        Assert.Equal("INTEGER", table.Columns[2].Type);
        Assert.False(table.Columns[2].IsPrimaryKey);
        Assert.True(table.Columns[2].Nullable);
        Assert.False(table.Columns[2].Unique);
    }

    [Fact]
    public void ShouldParseManyToManyFkColumns()
    {
        var table = _tqlParser.ParseTqlLine("Person-Address");

        Assert.Equal("Person_Address", table.Name);
        Assert.Equal(3, table.Columns.Count);
        Assert.Equal("Id", table.Columns[0].Name);
        Assert.Equal("INTEGER", table.Columns[0].Type);
        Assert.True(table.Columns[0].IsPrimaryKey);
        Assert.False(table.Columns[0].Nullable);
        Assert.False(table.Columns[0].Unique);
        Assert.Equal("PersonId", table.Columns[1].Name);
        Assert.Equal("INTEGER", table.Columns[1].Type);
        Assert.False(table.Columns[1].IsPrimaryKey);
        Assert.False(table.Columns[1].Nullable);
        Assert.False(table.Columns[1].Unique);
        Assert.Equal("AddressId", table.Columns[2].Name);
        Assert.Equal("INTEGER", table.Columns[2].Type);
        Assert.False(table.Columns[2].IsPrimaryKey);
        Assert.False(table.Columns[2].Nullable);
        Assert.False(table.Columns[2].Unique);
    }

    [Fact]
    public void AddsForeignKeysFromPrefix()
    {
        var table = _tqlParser.ParseTqlLine("Order>Customer>Product?(Date+)");

        Assert.Equal("Order", table.Name);

        var customerFK = table.Columns.FirstOrDefault(c => c.Name == "CustomerId");
        var productFK = table.Columns.FirstOrDefault(c => c.Name == "ProductId");

        Assert.NotNull(customerFK);
        Assert.NotNull(productFK);

        Assert.False(customerFK.Nullable);
        Assert.True(productFK.Nullable);
    }

    [Fact]
    public void SupportsForeignKeysWithoutOtherColumns()
    {
        var table = _tqlParser.ParseTqlLine("Invoice>Client>Currency?()");

        Assert.Equal("Invoice", table.Name);
        Assert.Equal(3, table.Columns.Count);
        Assert.Equal("Id", table.Columns[0].Name);
        Assert.Equal("INTEGER", table.Columns[0].Type);
        Assert.True(table.Columns[0].IsPrimaryKey);
        Assert.False(table.Columns[0].Nullable);
        Assert.Contains(table.Columns, c => c.Name == "ClientId");
        Assert.Contains(table.Columns, c => c.Name == "CurrencyId");
    }

    [Fact]
    public void SupportsForeignKeysWithPrimaryKeyOverride()
    {
        var table = _tqlParser.ParseTqlLine("Post>Author(Slug*, Title)");

        Assert.Equal("Post", table.Name);
        Assert.Equal(3, table.Columns.Count);
        Assert.Contains(table.Columns, c => c.Name == "AuthorId");
        Assert.DoesNotContain(table.Columns, c => c.Name == "Id"); // no default Id
        Assert.Contains(table.Columns, c => c.Name == "Slug" && c.IsPrimaryKey);
    }

    [Fact]
    public void SupportsNoForeignKeys()
    {
        var table = _tqlParser.ParseTqlLine("Tag(Name*)");

        Assert.Equal("Tag", table.Name);
        Assert.DoesNotContain(table.Columns, c => c.Name.EndsWith("Id"));
    }

    [Fact]
    public void CanChainMultipleForeignKeys()
    {
        var table = _tqlParser.ParseTqlLine("Booking>User>Room>Location?(Created+)");

        Assert.Equal("Booking", table.Name);

        Assert.Equal(5, table.Columns.Count);

        Assert.Contains(table.Columns, c => c.Name == "Id");        
        Assert.Contains(table.Columns, c => c.Name == "Created");
        Assert.Contains(table.Columns, c => c.Name == "UserId");
        Assert.Contains(table.Columns, c => c.Name == "RoomId");
        Assert.Contains(table.Columns, c => c.Name == "LocationId");

        Assert.True(table.Columns.First(c => c.Name == "LocationId").Nullable);
        Assert.False(table.Columns.First(c => c.Name == "UserId").Nullable);
    }
        
}
