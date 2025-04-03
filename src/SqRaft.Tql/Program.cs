
using Microsoft.Extensions.DependencyInjection;
using SqRaftEngine.Tql;
using SqRaftEngine.Tql.Abstractions;

var services = new ServiceCollection();
services.AddTqlEngine();
var serviceProvider = services.BuildServiceProvider();

var tqlPipeline = serviceProvider.GetRequiredService<ITqlPipeline>();

var input = args.Length > 0 ? args[0] : Console.ReadLine() ?? string.Empty;

var result = tqlPipeline.Execute(input);
if (result.Success)
{
    var codeResults = result.GeneratedCode;
    foreach (var codeResult in codeResults)
    {
        Console.WriteLine($"Language: {codeResult.Language}");
        Console.WriteLine($"Code: {codeResult.Code}");

        
    }
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
