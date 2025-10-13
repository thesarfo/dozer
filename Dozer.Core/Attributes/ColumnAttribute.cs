using System;

namespace Dozer.Core.Attributes;

/// <summary>
/// Maps a property to a specific database column name and optionally specifies the database type.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the database column.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the database-specific type for the column (e.g., "VARCHAR(100)", "DECIMAL(10,2)").
    /// </summary>
    public string DbType { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the database column. If null, the property name is used.</param>
    /// <param name="dbType">The database-specific type. If null, the type is inferred from the property type.</param>
    public ColumnAttribute(string name = null, string dbType = null)
    {
        Name = name;
        DbType = dbType;
    }
}