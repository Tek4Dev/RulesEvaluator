namespace RulesEvaluator.Core;

public record RuleResultTree
{
    public required bool IsSuccess { get; set; }
    public required RuleBase Rule { get; set; }

    public required Dictionary<string, bool> ChildResults { get; set; } = [];
    public string ExceptionMessage { get; set; }

}
