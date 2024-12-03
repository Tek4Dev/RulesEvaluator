using Microsoft.Extensions.Logging;

namespace RulesEvaluator.Core;

public class RulesEngine<T>(ILogger<RulesEngine<T>> Logger, DynamicPredicateEvaluator<T> evaluator)
{
    public List<RuleResultTree> ExecuteAll(List<Workflow> workflows, T model)
    {
       // var evaluator = new DynamicPredicateEvaluator<T>();
        var ruleResult = new List<RuleResultTree>();

        foreach (var workflow in workflows)
        {
            foreach (var rule in workflow.Rules)
            {
                var fullyQualifiedExpressionString = rule.Evaluate(model);
                var evalResults = evaluator.EvaluateStatement(fullyQualifiedExpressionString, model);
                ruleResult.Add(new RuleResultTree()
                {
                    Rule = rule,
                    IsSuccess = evalResults.Values.Last().ToString() == "True",
                    ChildResults = evalResults
                });
            }
        }

        return ruleResult;

    }
}
