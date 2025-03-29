using SqRaftEngine.Tql.Abstractions;
using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql;

public class TqlSemanticValidator : ITqlSemanticValidator
{
    public (bool IsValid, string? Error) Validate(TqlTable table)
    {
        // 1. No duplicate column names
        var duplicates = table.Columns
            .GroupBy(c => c.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count != 0)
            return (false, $"Duplicate column name(s): {string.Join(", ", duplicates)}");

        // 2. Multiple primary keys without composite
        var pkCount = table.Columns.Count(c => c.IsPrimaryKey);
        if (pkCount > 1 && !table.HasCompositeKey)
            return (false, $"Multiple primary keys defined but no composite key declared.");

        // 3. No primary key at all
        if (pkCount == 0 && !table.HasCompositeKey)
            return (false, $"No primary key defined.");

        // 4. Invalid combinations of suffixes (like ?!+-*)
        foreach (var col in table.Columns)
        {
            if (col.Name.Any(ch => char.IsWhiteSpace(ch)))
                return (false, $"Invalid whitespace in column name: '{col.Name}'");

            // You can extend with stricter suffix validation later
        }

        return (true, null);
    }
}
