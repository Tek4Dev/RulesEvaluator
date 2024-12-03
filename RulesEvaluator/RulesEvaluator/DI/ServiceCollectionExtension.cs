using Microsoft.Extensions.DependencyInjection;
using RulesEvaluator.Core;

namespace RulesEvaluator.DI;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRulesEngine(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton(typeof(PredicateEvaluatorExtensions));
        services.AddSingleton(typeof(DynamicPredicateEvaluator<>));
        services.AddSingleton(typeof(RulesEngine<>));
        return services;
    }
}
