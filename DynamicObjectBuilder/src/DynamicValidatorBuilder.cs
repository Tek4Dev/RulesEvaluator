using FluentValidation;
using FluentValidation.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

public static class DynamicValidatorBuilder
{
    public static Dictionary<Type, IValidator> GenerateValidators(Dictionary<string, Type> generatedTypes, Dictionary<string, ClassMetadata> hierarchy)
    {
        var validators = new Dictionary<Type, IValidator>();
        var assemblyName = new AssemblyName("DynamicValidators");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        foreach (var kvp in generatedTypes)
        {
            var type = kvp.Value;
            var metadata = hierarchy[kvp.Key];

            // Define a concrete validator class dynamically
            var validatorType = typeof(AbstractValidator<>).MakeGenericType(type);
            var validatorBuilder = moduleBuilder.DefineType(
                $"{type.Name}Validator",
                TypeAttributes.Public | TypeAttributes.Class,
                validatorType);

            // Create the validator type
            var concreteValidatorType = validatorBuilder.CreateType();

            // Instantiate the validator
            var validatorInstance = Activator.CreateInstance(concreteValidatorType);
            if (validatorInstance == null)
            {
                throw new InvalidOperationException($"Failed to create validator instance for type {type.Name}");
            }

            // Add validation rules dynamically
            foreach (var property in metadata.Properties)
            {
                var ruleForMethod = validatorType.GetMethod("RuleFor");
                if (ruleForMethod == null) continue;

                // Make the generic method concrete for the property type
                var propertyType = property.PropertyType;
                var ruleForGenericMethod = ruleForMethod.MakeGenericMethod(propertyType);

                // Create the lambda expression (x => x.PropertyName)
                var lambdaParam = Expression.Parameter(type, "x");
                var lambdaBody = Expression.Property(lambdaParam, property.PropertyName);
                var lambda = Expression.Lambda(lambdaBody, lambdaParam);

                // Invoke RuleFor(x => x.PropertyName)
                var ruleBuilder = ruleForGenericMethod.Invoke(validatorInstance, new object[] { lambda });

                if (ruleBuilder == null) continue;

                // Add string-specific validations
                if (propertyType == typeof(string))
                {
                    if (property.MaxLength.HasValue)
                    {
                        // Locate MaximumLength method for IRuleBuilderOptions<T, string>
                        var maximumLengthMethod = typeof(DefaultValidatorExtensions)
                            .GetMethod("MaximumLength", BindingFlags.Static | BindingFlags.Public)
                            ?.MakeGenericMethod(type);

                        maximumLengthMethod?.Invoke(null, new[] { ruleBuilder, property.MaxLength.Value });
                    }
                    if (property.IsRequired)
                    {
                        // Locate and invoke the NotEmpty method for IRuleBuilderOptions<T, string>
                        var notEmptyMethod = typeof(DefaultValidatorExtensions)
                            .GetMethod("NotEmpty", BindingFlags.Static | BindingFlags.Public)
                            ?.MakeGenericMethod(type, propertyType); // Resolve generics for the specific type and property type

                        if (notEmptyMethod != null)
                        {
                            notEmptyMethod.Invoke(null, new[] { ruleBuilder });
                        }
                        else
                        {
                            throw new InvalidOperationException("NotEmpty method could not be resolved for the property.");
                        }
                    }
                }

                //// Add numeric-specific validations
                //if (propertyType == typeof(int) || propertyType == typeof(decimal) || propertyType == typeof(float) || propertyType == typeof(double))
                //{
                //    if (property.MinValue.HasValue)
                //    {
                //        // Locate GreaterThanOrEqualTo method
                //        var greaterThanOrEqualToMethod = typeof(DefaultValidatorExtensions)
                //            .GetMethod("GreaterThanOrEqualTo", BindingFlags.Static | BindingFlags.Public)
                //            ?.MakeGenericMethod(type);

                //        greaterThanOrEqualToMethod?.Invoke(null, new[] { ruleBuilder, property.MinValue.Value });
                //    }
                //    if (property.MaxValue.HasValue)
                //    {
                //        // Locate LessThanOrEqualTo method
                //        var lessThanOrEqualToMethod = typeof(DefaultValidatorExtensions)
                //            .GetMethod("LessThanOrEqualTo", BindingFlags.Static | BindingFlags.Public)
                //            ?.MakeGenericMethod(type);

                //        lessThanOrEqualToMethod?.Invoke(null, new[] { ruleBuilder, property.MaxValue.Value });
                //    }
                //}

                // Add required validation
                //if (property.IsRequired)
                //{
                //    // Locate and invoke the NotEmpty method for IRuleBuilderOptions<T, string>
                //    var notEmptyMethod = typeof(DefaultValidatorExtensions)
                //        .GetMethod("NotEmpty", BindingFlags.Static | BindingFlags.Public)
                //        ?.MakeGenericMethod(type, propertyType);

                //    notEmptyMethod?.Invoke(null, new[] { ruleBuilder });
                //}
            }

            // Add the validator to the dictionary
            validators[type] = validatorInstance as IValidator;
        }

        return validators;
    }
}
