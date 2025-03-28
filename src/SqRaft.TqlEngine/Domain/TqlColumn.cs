namespace SqRaftEngine.Tql.Domain;

public class TqlColumn
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "TEXT";
    public bool Nullable { get; set; }
    public bool Unique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
}