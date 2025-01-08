using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

public static class DynamicTypeBuilder
{
    public static (Dictionary<string, Type> Types, Dictionary<string, object> Instances) BuildTypes(Dictionary<string, ClassMetadata> hierarchy)
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        var typeBuilders = new Dictionary<string, TypeBuilder>();
        var generatedTypes = new Dictionary<string, Type>();
        var instances = new Dictionary<string, object>();

        // Step 1: Create TypeBuilders for each class
        foreach (var kvp in hierarchy)
        {
            typeBuilders[kvp.Key] = moduleBuilder.DefineType(kvp.Key, TypeAttributes.Public | TypeAttributes.Class);
        }

        // Step 2: Define properties for each class
        foreach (var kvp in hierarchy)
        {
            var classMetadata = kvp.Value;
            var typeBuilder = typeBuilders[kvp.Key];

            // Add regular properties
            foreach (var prop in classMetadata.Properties)
            {
                DefineProperty(typeBuilder, prop);
            }

            // Add child references (as lists or objects)
            foreach (var childName in classMetadata.ChildClassNames)
            {
                if (typeBuilders.ContainsKey(childName))
                {
                    var childType = typeBuilders[childName];
                    DefineChildReference(typeBuilder, childName, childType, isCollection: true); // Create List<ChildType>
                }
            }
        }

        // Step 3: Create all types
        foreach (var kvp in typeBuilders)
        {
            generatedTypes[kvp.Key] = kvp.Value.CreateType();
        }

        // Step 4: Create instances for root-level classes
        foreach (var kvp in hierarchy)
        {
            if (string.IsNullOrEmpty(kvp.Value.ParentClassName)) // Root-level classes
            {
                Console.WriteLine($"Creating instance for root-level class: {kvp.Key}");
                var rootType = generatedTypes[kvp.Key];
                var instance = Activator.CreateInstance(rootType);
                InitializeCollectionsAndChildren(instance, kvp.Value, generatedTypes, hierarchy);
                instances[kvp.Key] = instance;
            }
        }

        Console.WriteLine($"Instances created: {instances.Count}");
        return (generatedTypes, instances);
    }

    private static void InitializeCollectionsAndChildren(object instance, ClassMetadata metadata, Dictionary<string, Type> generatedTypes, Dictionary<string, ClassMetadata> hierarchy)
    {
        var instanceType = instance.GetType();

        foreach (var childName in metadata.ChildClassNames)
        {
            if (generatedTypes.TryGetValue(childName, out var childType))
            {
                var property = instanceType.GetProperty(childName);

                if (property != null)
                {
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        // Initialize the collection
                        var collectionInstance = Activator.CreateInstance(property.PropertyType);
                        property.SetValue(instance, collectionInstance);

                        // Add a default child object to the collection
                        var childInstance = Activator.CreateInstance(childType);
                        InitializeCollectionsAndChildren(childInstance, hierarchy[childName], generatedTypes, hierarchy);
                        var addMethod = property.PropertyType.GetMethod("Add");
                        addMethod?.Invoke(collectionInstance, new[] { childInstance });
                    }
                    else
                    {
                        // Initialize one-to-one child object
                        var childInstance = Activator.CreateInstance(childType);
                        InitializeCollectionsAndChildren(childInstance, hierarchy[childName], generatedTypes, hierarchy);
                        property.SetValue(instance, childInstance);
                    }
                }
            }
        }
    }

    private static void DefineProperty(TypeBuilder typeBuilder, ClassPropertyMetadata prop)
    {
        var field = typeBuilder.DefineField($"_{prop.PropertyName}", prop.PropertyType, FieldAttributes.Private);
        var property = typeBuilder.DefineProperty(prop.PropertyName, PropertyAttributes.HasDefault, prop.PropertyType, null);

        var getter = typeBuilder.DefineMethod($"get_{prop.PropertyName}", MethodAttributes.Public, prop.PropertyType, Type.EmptyTypes);
        var setter = typeBuilder.DefineMethod($"set_{prop.PropertyName}", MethodAttributes.Public, null, new[] { prop.PropertyType });

        var getterIL = getter.GetILGenerator();
        getterIL.Emit(OpCodes.Ldarg_0);
        getterIL.Emit(OpCodes.Ldfld, field);
        getterIL.Emit(OpCodes.Ret);

        var setterIL = setter.GetILGenerator();
        setterIL.Emit(OpCodes.Ldarg_0);
        setterIL.Emit(OpCodes.Ldarg_1);
        setterIL.Emit(OpCodes.Stfld, field);
        setterIL.Emit(OpCodes.Ret);

        property.SetGetMethod(getter);
        property.SetSetMethod(setter);
    }

    private static void DefineChildReference(TypeBuilder parentBuilder, string childName, TypeBuilder childType, bool isCollection)
    {
        Type propertyType = isCollection ? typeof(List<>).MakeGenericType(childType) : childType;

        var field = parentBuilder.DefineField($"_{childName}", propertyType, FieldAttributes.Private);
        var property = parentBuilder.DefineProperty(childName, PropertyAttributes.HasDefault, propertyType, null);

        var getter = parentBuilder.DefineMethod($"get_{childName}", MethodAttributes.Public, propertyType, Type.EmptyTypes);
        var setter = parentBuilder.DefineMethod($"set_{childName}", MethodAttributes.Public, null, new[] { propertyType });

        var getterIL = getter.GetILGenerator();
        getterIL.Emit(OpCodes.Ldarg_0);
        getterIL.Emit(OpCodes.Ldfld, field);
        getterIL.Emit(OpCodes.Ret);

        var setterIL = setter.GetILGenerator();
        setterIL.Emit(OpCodes.Ldarg_0);
        setterIL.Emit(OpCodes.Ldarg_1);
        setterIL.Emit(OpCodes.Stfld, field);
        setterIL.Emit(OpCodes.Ret);

        property.SetGetMethod(getter);
        property.SetSetMethod(setter);

        // Automatically initialize the collection in the constructor if it's a list
        if (isCollection)
        {
            // Define the constructor for the parent type
            var ctor = parentBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctorIL = ctor.GetILGenerator();

            // Call the base object constructor
            ctorIL.Emit(OpCodes.Ldarg_0);
            var objCtor = typeof(object).GetConstructor(Type.EmptyTypes);
            ctorIL.Emit(OpCodes.Call, objCtor);

            // Initialize the List<ChildType> field
            ctorIL.Emit(OpCodes.Ldarg_0);

            // Resolve the closed generic type for List<ChildType>
            var genericListType = typeof(List<>).MakeGenericType(childType);

            // Use the static Activator.CreateInstance method to instantiate the generic list
            var activatorMethod = typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new[] { typeof(Type) });
            ctorIL.Emit(OpCodes.Ldtoken, genericListType);
            ctorIL.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
            ctorIL.Emit(OpCodes.Call, activatorMethod);
            ctorIL.Emit(OpCodes.Castclass, genericListType); // Cast the result to List<ChildType>

            ctorIL.Emit(OpCodes.Stfld, field); // Assign the list instance to the field

            ctorIL.Emit(OpCodes.Ret);
        }
    }

}
