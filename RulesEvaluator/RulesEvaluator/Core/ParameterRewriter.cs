﻿using System.Linq.Expressions;

namespace RulesEvaluator.Core;

public class ParameterRewriter : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ParameterRewriter(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}