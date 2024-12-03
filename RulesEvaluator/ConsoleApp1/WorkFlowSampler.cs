using Microsoft.Extensions.DependencyInjection;
using RulesEvaluator.Core;

namespace ConsoleApp1;
internal class WorkFlowSampler(RulesEngine<SampleModel> re)
{
    public void RunRuleEngine()
    {
        //var re = services.GetRequiredService<RulesEngine<SampleModel>>();
        // Model instance
        var model = new SampleModel
        {
            Username = "bc",
            Username2 = "ab",
            A = 0,
            B = 3
        };

        for (int i = 0; i < 1000; i++)
        {
            var results = re.ExecuteAll(RuleDefinitions.GetWorkflows(), model);
            Console.WriteLine("Detailed Results:");
            foreach (var result in results)
            {
                Console.WriteLine($"====Processed rule: {result.Rule.RuleName} Status: {result.IsSuccess} ErrorMsg: {result.Rule.ErrorMessage}====");
                foreach (var r in result.ChildResults)
                {
                    // Console.WriteLine($"{r.Key}: {r.Value}");
                }
                Console.WriteLine("------------");
            }
        }


    }
}
