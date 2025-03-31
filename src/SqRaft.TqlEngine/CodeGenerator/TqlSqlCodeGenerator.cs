using System.Text;
using SqRaftEngine.Tql.Abstractions;
using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql.CodeGenerator;

public class TqlSqlCodeGenerator : ITqlCondeGenerator
{
    //public string OutputName(TqlTable table) => $"{table.Name}.sql";

    public TqlGeneratedCode Generate(TqlTable table)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE {table.Name} (");

        var columnLines = new List<string>();

        foreach (var col in table.Columns)
        {
            var line = $"  {col.Name} {col.Type}";
            if (!col.Nullable) line += " NOT NULL";
            if (col.Unique) line += " UNIQUE";
            if (col.IsPrimaryKey && !table.HasCompositeKey) line += " PRIMARY KEY";
            columnLines.Add(line);
        }

        // Composite PK
        if (table.HasCompositeKey)
        {
            columnLines.Add($"  PRIMARY KEY ({string.Join(", ", table.CompositeKey)})");
        }

        // FKs (shallow)
        foreach (var col in table.Columns.Where(c => c.IsForeignKey))
        {
            columnLines.Add($"  ,FOREIGN KEY ({col.Name}) REFERENCES {col.ForeignKeyTable}({col.ForeignKeyColumn ?? "Id"})");
        }

        sb.AppendLine(string.Join(",\n", columnLines));
        sb.AppendLine(");");

        return new TqlGeneratedCode
        {
            Language = "SQL",
            Code = sb.ToString(),
            FileName = $"{table.Name}.sql"
        };
    }
}
