using System;

namespace Dozer.Core.Attributes;

/// <summary>
/// Marks a property as the primary key for an entity.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute
{
    /// <summary>
    /// Gets a value indicating whether the key should auto-increment.
    /// </summary>
    public bool IsAutoIncrement { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyAttribute"/> class.
    /// </summary>
    /// <param name="isAutoIncrement">Indicates whether the key should auto-increment. Default is true.</param>
    public KeyAttribute(bool isAutoIncrement = true)
    {
        IsAutoIncrement = isAutoIncrement;
    }
} 