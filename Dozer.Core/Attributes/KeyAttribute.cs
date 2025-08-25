using System;


namespace Dozer.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute
{
    public bool IsAutoIncrement { get; }

    public KeyAttribute(bool isAutoIncrement = true)
    {
        IsAutoIncrement = isAutoIncrement;
    }
} 