using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace RulesEvaluator.Core;
public class PredicateEvaluatorExtensions(ILogger<PredicateEvaluatorExtensions> Logger)
{
    public Expression<Func<T, bool>> ParsePredicate<T>(string predicateDescription, T modelInstance)
    {
        Logger.LogDebug($"Started {nameof(ParsePredicate)}");

        var adapter = new ModelAdapter<T>(modelInstance);

        // Replace methods with evaluated results
        string processedPredicate = ReplaceMethodCalls(predicateDescription, adapter);

        var config = new ParsingConfig
        {
            CustomTypeProvider = new CustomTypeProvider([typeof(SampleModel), typeof(string), typeof(RuleHelper)]),
        };

        // Parse the updated predicate string using Dynamic LINQ
        try
        {
            var parsedExpression = DynamicExpressionParser.ParseLambda<T, bool>(
               config,
                false,
                processedPredicate
            );

            //var parsedExpression = DynamicExpressionParser.ParseLambda<T, bool>(config, false, predicateDescription);

            var newParameter = Expression.Parameter(typeof(T), "model");

            var rewriter = new ParameterRewriter(parsedExpression.Parameters[0], newParameter);

            var updatedBody = rewriter.Visit(parsedExpression.Body);
            
            Logger.LogDebug($"Started {nameof(ParsePredicate)}");

            return Expression.Lambda<Func<T, bool>>(updatedBody, newParameter);

        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse predicate: {predicateDescription}. Error: {ex.Message}", ex);
        }
    }

    private static string ReplaceMethodCalls<T>(string predicate, ModelAdapter<T> adapter)
    {
        return predicate;

        //var type = typeof(T);
        //var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        //foreach (var method in methods)
        //{
        //    if (predicate.Contains($"{method.Name}()"))
        //    {
        //        // Replace the method call with its evaluated result
        //        var result = adapter.GetProperty(method.Name);
        //        predicate = predicate.Replace($"{method.Name}()", result.ToString());
        //    }
        //}

        //return predicate;
    }
}
