namespace RulesEvaluator.Core;

public class RuleHelper
{
    public static bool HasValue(string value)
    { return !string.IsNullOrEmpty(value); }

    //public static bool Equals(string source, string dest)
    //{
    //    return source.Equals(dest, StringComparison.OrdinalIgnoreCase);
    //}

    public static bool Contains(int id, int[] vals)
    {
        return vals.Length > 0 && vals.Contains(id);
    }
}
