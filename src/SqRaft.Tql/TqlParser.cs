using SqRaft.Tql.Abstractions;
using SqRaft.Tql.Domain;

namespace SqRaft.Tql;

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
        else
        {
            table.Name = rawName;
        }
    }
}
