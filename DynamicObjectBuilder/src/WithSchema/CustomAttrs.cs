using System;

[AttributeUsage(AttributeTargets.Property)]
public class DateFormatAttribute : Attribute
{
    public string Format { get; }
    public DateFormatAttribute(string format)
    {
        Format = format;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class StringLengthAttribute : Attribute
{
    public int Length { get; }
    public StringLengthAttribute(int length)
    {
        Length = length;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class DecimalPlacesAttribute : Attribute
{
    public int Places { get; }
    public DecimalPlacesAttribute(int places)
    {
        Places = places;
    }
}
