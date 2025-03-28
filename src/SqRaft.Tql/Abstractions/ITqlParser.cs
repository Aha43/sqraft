using SqRaft.Tql.Domain;

namespace SqRaft.Tql.Abstractions;

public interface ITqlParser
{
    /// <summary>
    /// Parses a TQL line and returns a TqlTable object.
    /// </summary>
    /// <param name="input">The TQL line to parse.</param>
    /// <returns>A TqlTable object representing the parsed TQL line.</returns>
    TqlTable ParseTqlLine(string input);
}
