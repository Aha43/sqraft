using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql.Abstractions;

public interface ITqlPipeline
{
    TqlPipelineResult Execute(string input);
}
