using SqRaftEngine.Tql.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace SqRaftEngine.Tql;

public static class Services
{
    public static IServiceCollection AddTqlEngine(this IServiceCollection services)
    {
        services.AddSingleton<ITqlParser, TqlParser>();
        
        return services;
    }
}
