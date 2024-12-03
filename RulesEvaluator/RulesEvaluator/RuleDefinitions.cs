using ConsoleApp1.Core;
using System.Linq.Expressions;
using static ConsoleApp1.Core.RuleHelper;

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

        var rulesList = new List<Rule>
        {
            new() { Id="1", RuleName="Rule1", Expression = predicate1, ErrorMessage = "Rule 1 failed" },
            new() { Id="2", RuleName="Rule2",Expression = predicate2, ErrorMessage = "Rule 2 failed" } ,
            new() { Id="3", RuleName="Rule3",Expression = predicate3 , ErrorMessage = "Rule 3 failed"}
        };

        var wf = new Workflow() { Rules = rulesList };
        return [wf];
    }


}





