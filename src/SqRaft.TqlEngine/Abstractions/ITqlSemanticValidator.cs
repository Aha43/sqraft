using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql.Abstractions;

public interface ITqlSemanticValidator
{
    (bool IsValid, string? Error) Validate(TqlTable table);
}
