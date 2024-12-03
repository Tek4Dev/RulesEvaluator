using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace RulesEvaluator.Core;

public class DynamicPredicateEvaluator<T>(ILogger<DynamicPredicateEvaluator<T>> Logger, PredicateEvaluatorExtensions PredicateEvaluator)
{
    public Dictionary<string, bool> Evaluate(Expression<Func<T, bool>> compositePredicate, T model)
    {
        Logger.LogDebug($"Started {nameof(Evaluate)}");
        // Dictionary to track results for individual predicates and groups
        var results = new Dictionary<string, bool>();

        // Evaluate the composite predicate and track all results
        EvaluateExpression(compositePredicate.Body, model, compositePredicate.Parameters[0], results);

        // Add the final composite result
        var compositeKey = compositePredicate.Body.ToString();
        if (!results.ContainsKey(compositeKey))
        {
            results[compositeKey] = compositePredicate.Compile()(model);
        }
        Logger.LogDebug($"Completed {nameof(Evaluate)}");

        return results;
    }

    public Dictionary<string, bool> EvaluateStatement(string predicateString, T model)
    {
        // Parse the predicate string into an Expression<Func<T, bool>>
        var parsedPredicate = PredicateEvaluator.ParsePredicate(predicateString, model);

        //var p = parsedPredicate.Compile();// (model);
        // Delegate to the existing Evaluate method
        return Evaluate(parsedPredicate, model);
    }

    private void EvaluateExpression(Expression expression, T model, ParameterExpression parameter, Dictionary<string, bool> results)
    {
        switch (expression)
        {
            case BinaryExpression binaryExpr:
                EvaluateBinaryExpression(binaryExpr, model, parameter, results);
                break;

            case MemberExpression memberExpr:
                EvaluateBooleanMemberExpression(memberExpr, model, parameter, results);
                break;

            case ConstantExpression constExpr:
                break; // Constants do not produce results to track

            case MethodCallExpression methodCallExpr:
                EvaluateMethodCallExpression(methodCallExpr, model, parameter, results);
                break;

            default:
                throw new NotSupportedException($"Unsupported expression type: {expression.GetType()}");
        }
    }

    private void EvaluateBinaryExpression(BinaryExpression binaryExpr, T model, ParameterExpression parameter, Dictionary<string, bool> results)
    {
        // Evaluate the left and right sides
        EvaluateExpression(binaryExpr.Left, model, parameter, results);
        EvaluateExpression(binaryExpr.Right, model, parameter, results);

        // Combine results based on the operator
        var leftValue = GetValue(binaryExpr.Left, model, parameter);
        var rightValue = GetValue(binaryExpr.Right, model, parameter);

        var result = binaryExpr.NodeType switch
        {
            ExpressionType.AndAlso => (bool)leftValue && (bool)rightValue,
            ExpressionType.OrElse => (bool)leftValue || (bool)rightValue,
            ExpressionType.Equal => Equals(leftValue, rightValue),
            ExpressionType.NotEqual => !Equals(leftValue, rightValue),
            ExpressionType.GreaterThan => Comparer<object>.Default.Compare(leftValue, rightValue) > 0,
            ExpressionType.GreaterThanOrEqual => Comparer<object>.Default.Compare(leftValue, rightValue) >= 0,
            ExpressionType.LessThan => Comparer<object>.Default.Compare(leftValue, rightValue) < 0,
            ExpressionType.LessThanOrEqual => Comparer<object>.Default.Compare(leftValue, rightValue) <= 0,
            _ => throw new NotSupportedException($"Unsupported binary operator: {binaryExpr.NodeType}")
        };

        // Record the result for this logical group or individual condition
        var description = binaryExpr.ToString();
        if (!results.ContainsKey(description))
        {
            results[description] = result;
        }
    }

    private void EvaluateBooleanMemberExpression(MemberExpression memberExpr, T model, ParameterExpression parameter, Dictionary<string, bool> results)
    {
        // Only evaluate boolean members directly; others must be part of a logical expression
        if (memberExpr.Type != typeof(bool))
        {
            return; // Non-boolean members are not standalone conditions
        }

        // Compile and evaluate the boolean member
        var lambda = Expression.Lambda<Func<T, bool>>(memberExpr, parameter);
        var compiled = lambda.Compile();
        var result = compiled(model);

        // Record the result
        var description = memberExpr.ToString();
        if (!results.ContainsKey(description))
        {
            results[description] = result;
        }
    }

    private void EvaluateMethodCallExpression(MethodCallExpression methodCallExpr, T model, ParameterExpression parameter, Dictionary<string, bool> results)
    {
        // Compile and evaluate the method call expression
        var lambda = Expression.Lambda<Func<T, bool>>(methodCallExpr, parameter);
        var compiled = lambda.Compile();
        var result = compiled(model);

        // Record the result
        var description = methodCallExpr.ToString();
        if (!results.ContainsKey(description))
        {
            results[description] = result;
        }
    }

    private object? GetValue(Expression expression, T model, ParameterExpression parameter)
    {
        switch (expression)
        {
            case MemberExpression memberExpr:
                // Compile and evaluate the member expression
                var memberLambda = Expression.Lambda(memberExpr, parameter);
                var memberCompiled = memberLambda.Compile();
                return memberCompiled.DynamicInvoke(model);

            case ConstantExpression constExpr:
                return constExpr.Value;

            case MethodCallExpression methodCallExpr:
                // Compile and evaluate the method call expression
                var methodLambda = Expression.Lambda(methodCallExpr, parameter);
                var methodCompiled = methodLambda.Compile();
                return methodCompiled.DynamicInvoke(model);

            case BinaryExpression binaryExpr:
                // Evaluate the binary expression
                return EvaluateBinaryExpressionForValue(binaryExpr, model, parameter);

            default:
                throw new NotSupportedException($"Unsupported expression type for value extraction: {expression.GetType()}");
        }
    }

    private object EvaluateBinaryExpressionForValue(BinaryExpression binaryExpr, T model, ParameterExpression parameter)
    {
        var leftValue = GetValue(binaryExpr.Left, model, parameter);
        var rightValue = GetValue(binaryExpr.Right, model, parameter);

        return binaryExpr.NodeType switch
        {
            ExpressionType.AndAlso => (bool)leftValue && (bool)rightValue,
            ExpressionType.OrElse => (bool)leftValue || (bool)rightValue,
            ExpressionType.Equal => Equals(leftValue, rightValue),
            ExpressionType.NotEqual => !Equals(leftValue, rightValue),
            ExpressionType.GreaterThan => Comparer<object>.Default.Compare(leftValue, rightValue) > 0,
            ExpressionType.GreaterThanOrEqual => Comparer<object>.Default.Compare(leftValue, rightValue) >= 0,
            ExpressionType.LessThan => Comparer<object>.Default.Compare(leftValue, rightValue) < 0,
            ExpressionType.LessThanOrEqual => Comparer<object>.Default.Compare(leftValue, rightValue) <= 0,
            _ => throw new NotSupportedException($"Unsupported binary operator: {binaryExpr.NodeType}")
        };
    }
}