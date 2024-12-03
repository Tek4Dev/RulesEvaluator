//using System.Reflection;

namespace RulesEvaluator.Core;

public class ModelAdapter<T>
{
    private readonly T _instance;

    public ModelAdapter(T instance)
    {
        _instance = instance;
    }

    public object GetProperty(string propertyName)
    {
        //var method = typeof(T).GetMethod(propertyName, BindingFlags.Public | BindingFlags.Instance);
        //if (method != null)
        //{
        //    // Invoke the method and return its result
        //    return method.Invoke(_instance, null);
        //}

        throw new InvalidOperationException($"Property or method '{propertyName}' not found in {typeof(T).Name}");
    }
}
