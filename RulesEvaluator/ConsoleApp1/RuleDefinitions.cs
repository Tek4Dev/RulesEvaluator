using RulesEvaluator.Core;
using System.Linq.Expressions;
using static RulesEvaluator.Core.RuleHelper;

namespace ConsoleApp1;

internal class RuleDefinitions
{
    public static List<Workflow> GetWorkflows()
    {

        // Complex predicate with nested logical conditions
        Expression<Func<SampleModel, bool>> predicate1 = model =>
                    string.IsNullOrEmpty(model.Username) == false   // string mtd
                    && model.Username.Length > 0 // left side exp is non bool
                    && (string.Equals("bc1", model.Username) || model.A == 0 || model.B == 2) // composite rule
                    && model.Customer.Id == 1 // child prop
                    && model.HasValue(model.Username)  // custom ext method
                    && model.Contains(model.Customer.Id, model.Countries) == true // passing array input
                    ;

        Expression<Func<SampleModel, bool>> predicate2 = model =>
                    model.HasValue(model.Username)  // custom ext method

                    // && model.Countries.Contains(1) == true  // array method call
                    && model.Equals("a", "a")
                    && model.Username == "kk" // non bool exp
                    ;

        Expression<Func<SampleModel, bool>> predicate3 = model =>
                    Equals("bc1", model.Username) || (model.A == 0 || model.B == 2)
                    && model.Customer.Id == 1
                    && HasValue(model.Username)
                    && model.Contains(model.Customer.Id, model.Countries);

        var wf = new Workflow();
        wf.Rules.Add(new Rule<SampleModel>() { Id = "1", RuleName = "Rule1", Expression = predicate1, ErrorMessage = "Rule 1 failed" });
        wf.Rules.Add(new Rule<SampleModel>() { Id = "2", RuleName = "Rule2", Expression = predicate2, ErrorMessage = "Rule 2 failed" });
        wf.Rules.Add(new Rule<SampleModel>() { Id = "3", RuleName = "Rule3", Expression = predicate3, ErrorMessage = "Rule 3 failed" });

        return [wf];
    }


}





