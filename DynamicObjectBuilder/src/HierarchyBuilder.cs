using System;
using System.Collections.Generic;

public class ClassPropertyMetadata
{
    public string PropertyName { get; set; }
    public Type PropertyType { get; set; }
    public int? MaxLength { get; set; }
    public bool IsCollection { get; set; }

    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public bool IsRequired { get; set; } = false;
}

public class ClassMetadata
{
    public string ClassName { get; set; }
    public string ParentClassName { get; set; }
    public List<ClassPropertyMetadata> Properties { get; set; } = new();
    public List<string> ChildClassNames { get; set; } = new(); // Track children
}

public static class HierarchyBuilder
{
    public static Dictionary<string, ClassMetadata> BuildHierarchy(List<ExcelClassDefinition> definitions)
    {
        var classMetadatas = new Dictionary<string, ClassMetadata>();

        foreach (var definition in definitions)
        {
            if (!classMetadatas.ContainsKey(definition.ClassName))
            {
                classMetadatas[definition.ClassName] = new ClassMetadata
                {
                    ClassName = definition.ClassName,
                    ParentClassName = definition.ParentClassName
                };
            }

            var propType = Type.GetType(definition.PropertyType);
            classMetadatas[definition.ClassName].Properties.Add(new ClassPropertyMetadata
            {
                PropertyName = definition.PropertyName,
                PropertyType = propType ?? typeof(string),
                MaxLength = definition.MaxLength,
                IsCollection = false, // This is updated later if it's a collection,
                IsRequired = definition.IsRequired
            });

            if (!string.IsNullOrEmpty(definition.ParentClassName))
            {
                if (!classMetadatas.ContainsKey(definition.ParentClassName))
                {
                    classMetadatas[definition.ParentClassName] = new ClassMetadata
                    {
                        ClassName = definition.ParentClassName
                    };
                }

                // Track child classes under parent
                if (!classMetadatas[definition.ParentClassName].ChildClassNames.Contains(definition.ClassName))
                {
                    classMetadatas[definition.ParentClassName].ChildClassNames.Add(definition.ClassName);
                }
            }
        }

        return classMetadatas;
    }
}
