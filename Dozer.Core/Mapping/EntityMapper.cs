using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dozer.Core.Attributes;

namespace Dozer.Core.Mapping;

public class EntityMapper<T> where T : class
{
    public string TableName { get; }
    public PropertyInfo KeyProperty { get; }
    public bool IsAutoIncrement { get; }
    public IReadOnlyDictionary<PropertyInfo, string> ColumnMappings { get; }
    public IReadOnlyDictionary<PropertyInfo, string> DbTypeMappings { get; }

    public EntityMapper()
    {
        var entityType = typeof(T);
        
        // Get table name
        var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
        TableName = tableAttr?.Name ?? entityType.Name;

        var properties = entityType.GetProperties();
        var columnMappings = new Dictionary<PropertyInfo, string>();
        var dbTypeMappings = new Dictionary<PropertyInfo, string>();

        // Find primary key
        var keyProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        if (keyProperty != null)
        {
            KeyProperty = keyProperty;
            IsAutoIncrement = keyProperty.GetCustomAttribute<KeyAttribute>().IsAutoIncrement;
        }

        // Map columns
        foreach (var property in properties)
        {
            var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr != null)
            {
                columnMappings[property] = columnAttr.Name ?? property.Name;
                if (!string.IsNullOrEmpty(columnAttr.DbType))
                {
                    dbTypeMappings[property] = columnAttr.DbType;
                }
                else
                {
                    dbTypeMappings[property] = MapCSharpTypeToSqlType(property.PropertyType);
                }
            }
            else
            {
                // Default mapping if no attribute is specified
                columnMappings[property] = property.Name;
                dbTypeMappings[property] = MapCSharpTypeToSqlType(property.PropertyType);
            }
        }

        ColumnMappings = columnMappings;
        DbTypeMappings = dbTypeMappings;
    }

    private string MapCSharpTypeToSqlType(Type type)
    {
        if (type == typeof(int))
            return "INT";
        if (type == typeof(long))
            return "BIGINT";
        if (type == typeof(string))
            return "NVARCHAR(MAX)";
        if (type == typeof(DateTime))
            return "DATETIME2";
        if (type == typeof(bool))
            return "BIT";
        if (type == typeof(decimal))
            return "DECIMAL(18,2)";
        if (type == typeof(double))
            return "FLOAT";
        if (type == typeof(Guid))
            return "UNIQUEIDENTIFIER";
        if (type.IsEnum)
            return "INT";
        if (type == typeof(byte[]))
            return "VARBINARY(MAX)";

        throw new NotSupportedException($"Type {type.Name} is not supported for SQL mapping");
    }
} 