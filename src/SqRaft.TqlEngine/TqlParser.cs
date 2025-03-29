using SqRaftEngine.Tql.Abstractions;
using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql;

public class TqlParser : ITqlParser
{
    public TqlTable ParseTqlLine(string input)
    {
        var open = input.IndexOf('(');
        var close = input.LastIndexOf(')');

        if (open == -1)
        {
            open = input.Length;
            close = input.Length;
        }

        var rawName = input[..open].Trim();
        var columns = open < close
            ? input[(open + 1)..close].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) : [];

        var table = new TqlTable();
        var userDefinedPK = false;

        table.Name = ExtractTableNameFromPrefix(rawName);

        AddManyToManyFkColumns(rawName, table);

        foreach (var colDef in columns)
        {
            var col = new TqlColumn();
            var end = colDef.IndexOfAny("?!+-*".ToCharArray());
            col.Name = end == -1 ? colDef : colDef[..end];

            if (end != -1)
            {
                foreach (var ch in colDef[end..])
                {
                    switch (ch)
                    {
                        case '?': col.Nullable = true; break;
                        case '!': col.Unique = true; break;
                        case '+': col.Type = "INTEGER"; break;
                        case '-': col.Type = "TEXT"; break;
                        case '*': col.IsPrimaryKey = true; userDefinedPK = true; break;
                    }
                }
            }

            if (col.Name == "Id" && !col.IsPrimaryKey)
            {
                col.Type = "INTEGER";
                col.IsPrimaryKey = true;
                userDefinedPK = true;
            }

            table.Columns.Add(col);
        }

        if (!rawName.Contains('+') && !rawName.Contains('-') && !userDefinedPK)
        {
            table.Columns.Insert(0, new TqlColumn { Name = "Id", Type = "INTEGER", IsPrimaryKey = true });
        }
        
        if (rawName.Contains('>'))
        {
            AddForeignKeyColumns(rawName, table);
        }

        return table;
    }

    private static void AddManyToManyFkColumns(string rawName, TqlTable table)
    {
        if (rawName.Contains('+') || rawName.Contains('-'))
        {
            var isComposite = rawName.Contains('+');
            var parts = rawName.Split(isComposite ? '+' : '-');
            table.Name = $"{parts[0]}_{parts[1]}";

            // Default FK columns (no type resolution here yet)
            table.Columns.Add(new TqlColumn { Name = parts[0] + "Id", Type = "INTEGER" });
            table.Columns.Add(new TqlColumn { Name = parts[1] + "Id", Type = "INTEGER" });

            if (isComposite)
                table.CompositeKey.AddRange([parts[0] + "Id", parts[1] + "Id"]);
            else
                table.Columns.Insert(0, new TqlColumn { Name = "Id", Type = "INTEGER", IsPrimaryKey = true });
        }
    }

    private static void AddForeignKeyColumns(string rawName, TqlTable table)
    {
        var foreignKeys = ParseForeignKeys(rawName);
        foreach (var (tableName, nullable) in foreignKeys)
        {
            var col = new TqlColumn
            {
                Name = tableName + "Id",
                Type = "INTEGER",
                IsForeignKey = true,
                ForeignKeyTable = tableName,
                ForeignKeyColumn = "Id",
                Nullable = nullable
            };
            table.Columns.Add(col);
        }
    }

    private static List<(string tableName, bool nullable)> ParseForeignKeys(string rawPrefix)
    {
        var retVal = new List<(string tableName, bool nullable)>();
        var segments = rawPrefix.Split('>').Skip(1);
        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            var tableName = trimmed.TrimEnd('?');
            var nullable = trimmed.EndsWith('?');
            retVal.Add((tableName, nullable));
        }
        return retVal;
    }

    private static string ExtractTableNameFromPrefix(string rawName)
    {
        var split = rawName.Split(['>', '-', '+'], StringSplitOptions.RemoveEmptyEntries);
        return split[0].Trim();
    }

}
