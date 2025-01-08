using System.Globalization;
using System.Reflection;

public static class CustomDeserializer
{
    public static TOutput Deserialize<TInput, TOutput>(TInput input) where TOutput : new()
    {
        var result = new TOutput(); // Create an instance of the output object
        var inputProperties = typeof(TInput).GetProperties();
        var outputProperties = typeof(TOutput).GetProperties();

        foreach (var outputProperty in outputProperties)
        {
            // Find matching input property by name
            var inputProperty = inputProperties.FirstOrDefault(p => p.Name == outputProperty.Name);
            if (inputProperty == null) continue;

            var stringValue = inputProperty.GetValue(input)?.ToString(); // Input value is always a string
            if (stringValue == null) continue;

            object convertedValue = stringValue;

            // DateFormat Attribute
            var dateFormatAttribute = outputProperty.GetCustomAttribute<DateFormatAttribute>();
            if (dateFormatAttribute != null && outputProperty.PropertyType == typeof(DateTime))
            {
                if (DateTime.TryParseExact(
             stringValue,
             dateFormatAttribute.Format,
             CultureInfo.InvariantCulture,
             DateTimeStyles.None,
             out var parsedDateTime))
                {
                    convertedValue = parsedDateTime;
                }
                else
                {
                    throw new FormatException(
                        $"Invalid date format for property '{outputProperty.Name}'. Expected format: {dateFormatAttribute.Format}, but got: '{stringValue}'."
                    );
                }
            }

            // StringLength Attribute
            var stringLengthAttribute = outputProperty.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttribute != null && outputProperty.PropertyType == typeof(string))
            {
                convertedValue = stringValue.Length > stringLengthAttribute.Length
                    ? stringValue.Substring(0, stringLengthAttribute.Length)
                    : stringValue;
            }

            // DecimalPlaces Attribute
            var decimalPlacesAttribute = outputProperty.GetCustomAttribute<DecimalPlacesAttribute>();
            if (decimalPlacesAttribute != null && outputProperty.PropertyType == typeof(decimal))
            {
                if (decimal.TryParse(stringValue, out var parsedDecimal))
                {
                    convertedValue = Math.Round(parsedDecimal, decimalPlacesAttribute.Places);
                }
                else
                {
                    throw new FormatException($"Invalid decimal format for property '{outputProperty.Name}'.");
                }
            }

            // Default Type Conversion
            if (outputProperty.PropertyType == typeof(int))
            {
                if (int.TryParse(stringValue, out var parsedInt))
                {
                    convertedValue = parsedInt;
                }
                else
                {
                    throw new FormatException($"Invalid integer format for property '{outputProperty.Name}'.");
                }
            }

            if (outputProperty.PropertyType == typeof(double))
            {
                if (double.TryParse(stringValue, out var parsedDouble))
                {
                    convertedValue = parsedDouble;
                }
                else
                {
                    throw new FormatException($"Invalid double format for property '{outputProperty.Name}'.");
                }
            }

            // Set the converted value to the result object
            outputProperty.SetValue(result, convertedValue);
        }

        return result;
    }
}
