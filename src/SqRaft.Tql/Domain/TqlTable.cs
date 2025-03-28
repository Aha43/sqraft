namespace SqRaft.Tql.Domain;

public class TqlTable
{
    public string Name { get; set; } = string.Empty;
    public List<TqlColumn> Columns { get; set; } = [];
    public bool HasCompositeKey => CompositeKey.Count > 0;
    public List<string> CompositeKey { get; set; } = [];
}
