using System.Text;
using SqRaftEngine.Tql.Abstractions;
using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql.CodeGenerator;

public class TqlTqlCodeGenerator : ITqlCondeGenerator
{
    public TqlGeneratedCode Generate(TqlTable table)
    {
        var code = GenerateCode(table);
        var generatedCode = new TqlGeneratedCode
        {
            Code = code,
            Language = "TQL",
            FileName = $"{table.Name}.tql"
        };
        return generatedCode;
    }

    private static string GenerateCode(TqlTable table)
    {
        var builder = new StringBuilder();
        builder.Append($"{table.Name}");

        if (table.Columns.Count > 0)
        {
            builder.Append('(');
            builder.Append(string.Join(", ", table.Columns.Select(ColToTql)));
            builder.Append(')');
        }

        return builder.ToString();
    }

    private static string ColToTql(TqlColumn col)
    {
        var suffix = new StringBuilder();
        if (col.Nullable) suffix.Append('?');
        if (col.Unique) suffix.Append('!');
        if (col.Type == "INTEGER") suffix.Append('+');
        if (col.Type == "TEXT") suffix.Append('-'); // optional
        if (col.IsPrimaryKey) suffix.Append('*');

        return col.Name + suffix;
    }
}
