using SqRaftEngine.Tql.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace SqRaftEngine.Tql;

public static class Services
{
    public static IServiceCollection AddTqlEngine(this IServiceCollection services)
    {
        services.AddSingleton<ITqlParser, TqlParser>()
            .AddSingleton<ITqlSyntaxValidator, RegexTqlSyntaxValidator>()
            .AddSingleton<ITqlSemanticValidator, TqlSemanticValidator>()
            .AddSingleton<ITqlCondeGenerator, TqlSqlCodeGenerator>()
            .AddSingleton<ITqlPipeline, TqlPipeline>();
        
        return services;
    }
}
