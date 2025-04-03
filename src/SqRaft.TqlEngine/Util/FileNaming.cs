using System.Text.RegularExpressions;

namespace SqRaftEngine.Tql.Util;

public static class FileNaming
{
    private static readonly Dictionary<string, Regex> languages = new()
    {
        { "sql", new Regex(@"^(\d+)_([A-Za-z0-9_]+)\.sql$", RegexOptions.IgnoreCase | RegexOptions.Compiled) }
    };

    public static string GetFilename(string lan, string tableName)
    {
        if (!languages.TryGetValue(lan, out Regex? OrderedFileRegex))
        {
            return $"{tableName}.{lan}"; // Fallback to default naming if no regex is found to match ordered files
        }
            
        Directory.CreateDirectory(lan);

        var existingFiles = Directory.GetFiles(lan, $"*.{lan}");
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

        return existingFile ?? $"{(maxIndex + 1):D4}_{tableName}.{lan}";
    }
}
