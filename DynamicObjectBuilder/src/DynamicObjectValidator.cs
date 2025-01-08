using FluentValidation;
using FluentValidation.Results;
using System;

public static class DynamicObjectValidator
{
    public static ValidationResult ValidateObject(object instance, Dictionary<Type, IValidator> validators)
    {
        var instanceType = instance.GetType();

        if (validators.TryGetValue(instanceType, out var validator))
        {
            try
            {
                // Create a ValidationContext<T> dynamically
                var validationContextType = typeof(ValidationContext<>).MakeGenericType(instanceType);
                var validationContext = Activator.CreateInstance(validationContextType, instance);

                // Perform validation
                var validateMethod = validator.GetType().GetMethod("Validate", new[] { validationContextType });
                if (validateMethod == null)
                {
                    throw new InvalidOperationException($"Validate method not found on validator for {instanceType.Name}");
                }

                var validationResult = (ValidationResult)validateMethod.Invoke(validator, new[] { validationContext });

                return validationResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during validation: {ex.Message}");
                throw;
            }
        }
        else
        {
            throw new InvalidOperationException($"No validator found for type {instanceType.Name}");
        }
    }
}
