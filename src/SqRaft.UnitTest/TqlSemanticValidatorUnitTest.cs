using SqRaftEngine.Tql;
using SqRaftEngine.Tql.Abstractions;
using SqRaftEngine.Tql.Domain;

namespace SqRaft.UnitTest;

public class TqlSemanticValidatorUnitTest
{
    private readonly ITqlSemanticValidator _validator = new TqlSemanticValidator();

    [Fact]
    public void ValidTable_PassesValidation()
    {
        var table = new TqlTable
        {
            Name = "User",
            Columns =
            {
                new TqlColumn { Name = "Id", Type = "INTEGER", IsPrimaryKey = true },
                new TqlColumn { Name = "Email", Type = "TEXT" }
            }
        };

        var (isValid, error) = _validator.Validate(table);
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void DuplicateColumns_FailsValidation()
    {
        var table = new TqlTable
        {
            Name = "User",
            Columns =
            {
                new TqlColumn { Name = "Email", Type = "TEXT" },
                new TqlColumn { Name = "Email", Type = "TEXT" }
            }
        };

        var (isValid, error) = _validator.Validate(table);
        Assert.False(isValid);
        Assert.Contains("Duplicate column", error);
    }

    [Fact]
    public void MultiplePrimaryKeysWithoutComposite_FailsValidation()
    {
        var table = new TqlTable
        {
            Name = "User",
            Columns =
            {
                new TqlColumn { Name = "Id", Type = "INTEGER", IsPrimaryKey = true },
                new TqlColumn { Name = "Code", Type = "TEXT", IsPrimaryKey = true }
            }
        };

        var (isValid, error) = _validator.Validate(table);
        Assert.False(isValid);
        Assert.Contains("Multiple primary keys", error);
    }

    [Fact]
    public void NoPrimaryKey_FailsValidation()
    {
        var table = new TqlTable
        {
            Name = "Note",
            Columns =
            {
                new TqlColumn { Name = "Title", Type = "TEXT" }
            }
        };

        var (isValid, error) = _validator.Validate(table);
        Assert.False(isValid);
        Assert.Contains("No primary key", error);
    }

    [Fact]
    public void CompositeKey_SkipsPrimaryKeyCheck()
    {
        var table = new TqlTable
        {
            Name = "JoinTable",
            CompositeKey = { "UserId", "ProjectId" },
            Columns =
            {
                new TqlColumn { Name = "UserId", Type = "INTEGER" },
                new TqlColumn { Name = "ProjectId", Type = "INTEGER" }
            }
        };

        var (isValid, error) = _validator.Validate(table);
        Assert.True(isValid);
        Assert.Null(error);
    }
}
