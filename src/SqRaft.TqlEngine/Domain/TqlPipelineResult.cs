namespace SqRaftEngine.Tql.Domain;

public class TqlPipelineResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TqlTable? Table { get; init; }
    public IEnumerable<TqlGeneratedCode> GeneratedCode { get; init; } = [];
}
