using System;

namespace Dozer.Core.Attributes;

/// <summary>
/// Specifies the database table name for an entity class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the database table.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TableAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the database table.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
    public TableAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}