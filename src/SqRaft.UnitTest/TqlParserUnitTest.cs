using SqRaft.Tql;
using SqRaft.Tql.Abstractions;

namespace SqRaft.UnitTest;

public class UnitTest1
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
        
}
