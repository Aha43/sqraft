using System;

namespace SqRaftEngine.Tql.Util;

public class FileNameOrdering
{

}

using System.Text.RegularExpressions;

public static class SqlFileNaming
{
    private static readonly Regex OrderedFileRegex = new(@"^(\d+)_([A-Za-z0-9_]+)\.sql$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string GetOrderedSqlFilename(string sqlDir, string tableName)
    {
        Directory.CreateDirectory(sqlDir);

        var existingFiles = Directory.GetFiles(sqlDir, "*.sql");
        int maxIndex = 0;
        string? existingFile = null;

        foreach (var filePath in existingFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var match = OrderedFileRegex.Match(fileName);
            if (match.Success)
            {
                var index = int.Parse(match.Groups[1].Value);
                var name = match.Groups[2].Value;

                if (name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    existingFile = fileName; // we’ll overwrite this
                    break;
                }

                maxIndex = Math.Max(maxIndex, index);
            }
        }

        return existingFile ?? $"{(maxIndex + 1):D4}_{tableName}.sql";
    }
}

