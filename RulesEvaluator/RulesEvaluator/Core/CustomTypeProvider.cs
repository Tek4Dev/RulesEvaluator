// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace RulesEvaluator.Core;

public class CustomTypeProvider : DefaultDynamicLinqCustomTypeProvider
{
    private HashSet<Type> _types;
    public CustomTypeProvider(Type[] types) : base(ParsingConfig.Default)
    {
        _types = new HashSet<Type>(types ?? new Type[] { });
        //_types.Add(typeof(ExpressionUtils));
    }

    public override HashSet<Type> GetCustomTypes()
    {
        return _types;
    }
}
