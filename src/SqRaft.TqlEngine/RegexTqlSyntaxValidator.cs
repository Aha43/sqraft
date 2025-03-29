using System.Text.RegularExpressions;
using SqRaftEngine.Tql.Abstractions;

namespace SqRaftEngine.Tql;

public class RegexTqlSyntaxValidator : ITqlSyntaxValidator
{
    private static readonly Regex ValidTqlPattern = new(
    @"^\s*
    (?<prefix>[A-Za-z_][A-Za-z0-9_]*([>+\-][A-Za-z_][A-Za-z0-9_]*\??)*)
    \s*
    (\(
        (?<columns>
            (\s*[A-Za-z_][A-Za-z0-9_]*[?!+\-*]*\s*
            (,)?\s*)*
        )
    \))?
    \s*$",
        RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

    public (bool isValid, string? errorMessage) Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (false, "Input is empty.");

        if (!ValidTqlPattern.IsMatch(input))
            return (false, "Input does not match TQL format.");

        return (true, null);
    }
}
