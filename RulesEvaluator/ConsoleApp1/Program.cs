using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RulesEvaluator.DI;
using System.Diagnostics;

namespace ConsoleApp1;

class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        var services = builder.Services;
        services.AddRulesEngine();
        services.AddSingleton<WorkFlowSampler>();
        var app = builder.Build();

        var sw = Stopwatch.StartNew();
        var wfSampler = app.Services.GetRequiredService<WorkFlowSampler>();
        wfSampler.RunRuleEngine();
        sw.Stop();

        Console.WriteLine($"Execution Time: {sw.ElapsedMilliseconds} ms");

        await app.RunAsync();

    }
}