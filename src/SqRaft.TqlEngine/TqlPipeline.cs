using SqRaftEngine.Tql.Abstractions;
using SqRaftEngine.Tql.Domain;

namespace SqRaftEngine.Tql;

public class TqlPipeline : ITqlPipeline
{
    private readonly ITqlSyntaxValidator _syntaxValidator;
    private readonly ITqlParser _parser;
    private readonly ITqlSemanticValidator _semanticValidator;

    private readonly IEnumerable<ITqlCondeGenerator> _codeGenerators;
    

    public TqlPipeline(
        ITqlSyntaxValidator syntaxValidator, 
        ITqlParser parser, 
        ITqlSemanticValidator semanticValidator,
        IEnumerable<ITqlCondeGenerator> codeGenerators)
    {
        _syntaxValidator = syntaxValidator;
        _parser = parser;
        _semanticValidator = semanticValidator;
        _codeGenerators = codeGenerators;
    }

    public TqlPipelineResult Execute(string input)
    {
        var (isValid, errorMessage) = _syntaxValidator.Validate(input);
        if (!isValid)
        {
            return new TqlPipelineResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        var parseResult = _parser.ParseTqlLine(input);
        if (parseResult is null)
        {
            return new TqlPipelineResult
            {
                Success = false,
                ErrorMessage = "Parsing failed."
            };
        }

        var (IsValid, Error) = _semanticValidator.Validate(parseResult);
        if (!IsValid)
        {
            return new TqlPipelineResult
            {
                Success = false,
                ErrorMessage = Error
            };
        }

        var codeResults = _codeGenerators.Select(generator => generator.Generate(parseResult)).ToList();

        return new TqlPipelineResult
        {
            Success = true,
            ErrorMessage = null,
            Table = parseResult,
            GeneratedCode = codeResults
        };
    }
}
