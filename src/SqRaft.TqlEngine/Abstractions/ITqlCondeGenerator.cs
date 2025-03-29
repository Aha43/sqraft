using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql.Abstractions;

public interface ITqlCondeGenerator
{
    TqlGeneratedCode Generate(TqlTable table);
}
