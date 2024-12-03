using System.Linq.Expressions;

namespace RulesEvaluator.Core;

public class Rule<T> : RuleBase
{
    //public required string RuleName { get; set; }
    //public required string Id { get; set; } // for storing the UniqueId
    //public string MessageId { get; set; }
    //public string Description { get; set; }
    //public required string ErrorMessage { get; set; }
    //public string Severity { get; set; }

    //public Rule(string name, string description, Expression<Func<T, bool>> expression)
    //{
    //    RuleName = name;
    //    Description = description;
    //    Expression = expression;
    //}

    public Expression<Func<T, bool>> Expression { get; set; }

    public override string Evaluate<TInput>(TInput model)
    {
        //var compiledExpression = Expression.Compile();
        //return compiledExpression((T)model);
        if (model is T)
        {
            var expressionStringBuilder = new FullyQualifiedNameVisitor();
            var fullyQualifiedExpressionString = expressionStringBuilder.GetExpressionString(Expression);

            return fullyQualifiedExpressionString;
        }
        return string.Empty;

    }
}


public abstract class RuleBase
{

    public required string RuleName { get; set; }
    public required string Id { get; set; } // for storing the UniqueId
    public string MessageId { get; set; }
    public string Description { get; set; }
    public required string ErrorMessage { get; set; }
    public string Severity { get; set; }

    public abstract string Evaluate<T>(T model);
}