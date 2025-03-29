namespace SqRaftEngine.Tql.Abstractions;

public interface ITqlSyntaxValidator
{
    (bool isValid, string? errorMessage) Validate(string input);
}
