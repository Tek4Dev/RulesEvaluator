public class SampleModel
{
    public string Username { get; set; }
    public string Username2 { get; set; }

    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }

    public int D { get; set; }

    public Customer Customer { get; set; } = new Customer();

    public readonly int[] Countries = [1, 2];

    //public bool IsNullOrEmpty(string? value)
    //{
    //    return string.IsNullOrWhiteSpace(value);
    //}

    public bool HasValue(string value)
    { return !string.IsNullOrEmpty(value); }

    public bool Equals(string source, string dest)
    {
        return source.Equals(dest, StringComparison.OrdinalIgnoreCase);
    }

    public bool Contains(int id, int[] vals)
    {
        return vals.Length > 0 && vals.Contains(id);
    }
}

public class Customer
{
    public int Id { get; set; } = 1;
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


