using SqRaftEngine.Tql;
using SqRaftEngine.Tql.Abstractions;

namespace SqRaft.UnitTest;

public class SyntaxValidatorUnitTest
{
    private readonly ITqlSyntaxValidator _validator = new RegexTqlSyntaxValidator();

    [Theory]
    [InlineData("User(Name, Email!)")]
    [InlineData("User>Company(Name, Email!)")]
    [InlineData("User>Company>Country?(Name, Email!)")]
    [InlineData("Tag(Name*)")]
    [InlineData("Post>Author(Slug*, Title)")]
    [InlineData("Person+Project(Role, Joined?)")]
    [InlineData("Note()")]
    [InlineData("Task")]
    public void ValidInputs_ReturnTrue(string input)
    {
        var (isValid, error) = _validator.Validate(input);
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("()")] // no table name
    [InlineData("User(")] // unmatched paren
    [InlineData("User)") ]
    [InlineData("User(Name,,Email)")] // double comma
    [InlineData("User(Name, ,Email)")] // empty column
    [InlineData("123User(Name)")] // invalid table name
    [InlineData("User(Name#)")] // invalid character in suffix
    [InlineData("User>Name>)")] // malformed foreign chain
    [InlineData("Post>Author>(Slug*, Title)")] // malformed foreign chain
    public void InvalidInputs_ReturnFalseWithError(string input)
    {
        var (isValid, error) = _validator.Validate(input);
        Assert.False(isValid);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}
