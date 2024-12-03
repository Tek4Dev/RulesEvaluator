using System.Linq.Expressions;
using System.Text;

namespace RulesEvaluator.Core;

public class FullyQualifiedNameVisitor : ExpressionVisitor
{
    private readonly StringBuilder _builder = new();

    public string GetExpressionString(Expression expression)
    {
        Visit(expression);
        return _builder.ToString();
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        _builder.Append(node.Parameters[0].Name);
        _builder.Append(" => ");
        Visit(node.Body);
        return node;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _builder.Append("(");
        Visit(node.Left);
        _builder.Append($" {GetOperator(node.NodeType)} ");
        Visit(node.Right);
        _builder.Append(")");
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Check if the method belongs to System.String
        if (node.Method.DeclaringType == typeof(string))
        {
            _builder.Append($"{node.Method.DeclaringType.FullName}.{node.Method.Name}(");
        }
        // Handle other methods (e.g., model.HasValue)
        else if (node.Method.IsStatic)
        {
            _builder.Append($"{node.Method.DeclaringType.FullName}.{node.Method.Name}(");
        }
        else
        {
            if (node.Object != null)
            {
                Visit(node.Object);
                _builder.Append($".{node.Method.Name}(");
            }
            else
            {
                _builder.Append($"{node.Method.Name}(");
            }
        }

        for (int i = 0; i < node.Arguments.Count; i++)
        {
            if (i > 0)
                _builder.Append(", ");
            Visit(node.Arguments[i]);
        }

        _builder.Append(")");
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null)
        {
            Visit(node.Expression);
            _builder.Append($".{node.Member.Name}");
        }
        else
        {
            _builder.Append(node.Member.Name);
        }

        return node;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        _builder.Append(node.Name);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        _builder.Append(node.Value is string ? $"\"{node.Value}\"" : node.Value);
        return node;
    }

    private string GetOperator(ExpressionType nodeType) =>
        nodeType switch
        {
            ExpressionType.AndAlso => "&&",
            ExpressionType.OrElse => "||",
            ExpressionType.Equal => "==",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Operator '{nodeType}' is not supported")
        };
}