using ConsoleApp1.Core;
using System.Diagnostics;

namespace ConsoleApp1;

class Program
{

    static void Main(string[] args)
    {


        // Model instance
        var model = new SampleModel
        {
            Username = "bc",
            Username2 = "ab",
            A = 0,
            B = 3
        };

        // Evaluate the predicate
        RulesEngine re = new(RuleDefinitions.GetWorkflows());
        var sw = Stopwatch.StartNew();

        //var item = RuleDefinitions.GetWorkflows().FirstOrDefault().Rules.FirstOrDefault();
        //var re = new RuleExpressionParser();
        //var re1 = re.Compile<SampleModel>(item.Expression.ToString(), [new("input1", model)]);

        for (int i = 0; i < 1; i++)
        {
            var results = re.ExecuteAll(model);
            Console.WriteLine("Detailed Results:");
            foreach (var result in results)
            {
                Console.WriteLine($"====Processed rule: {result.Rule.RuleName} Status: {result.IsSuccess} ErrorMsg: {result.Rule.ErrorMessage}====");
                foreach (var r in result.ChildResults)
                {
                    Console.WriteLine($"{r.Key}: {r.Value}");
                }
                Console.WriteLine("------------");
            }
        }
        sw.Stop();
        Console.WriteLine(sw.ElapsedMilliseconds);

    }
}


//public static void TestGroupExpression()
//{
//    // Create a parameter for the model
//    var parameter = Expression.Parameter(typeof(MyModel), "x");

//    var groupContent = "A == 10 || B == 234";

//    // Parse and build the expression
//    var evaluator = new DynamicPredicateEvaluator<MyModel>();
//    var groupExpression = evaluator.BuildGroupExpression(groupContent, parameter);

//    // Compile and test the group expression
//    var model = new MyModel { A = 0, B = 3 }; // A and B do not match the group
//    var compiled = groupExpression.Compile();
//    var result = compiled(model);

//    Console.WriteLine($"Group Expression: {groupExpression}");
//    Console.WriteLine($"Result for model {{ A = {model.A}, B = {model.B} }}: {result}");
//}


