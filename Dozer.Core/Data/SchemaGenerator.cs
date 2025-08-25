using System;
using System.Linq;
using System.Text;
using System.Reflection;
using Dozer.Core.Mapping;
using Dozer.Core.Attributes;

namespace Dozer.Core.Data;

public class SchemaGenerator<T> where T : class
{
    private readonly EntityMapper<T> _mapper;

    public SchemaGenerator(EntityMapper<T> mapper)
    {
        _mapper = mapper;
    }

    public string GenerateCreateTableSql()
    {
        var sql = new StringBuilder();
        sql.AppendLine($"CREATE TABLE IF NOT EXISTS {_mapper.TableName} (");

        var columnDefinitions = _mapper.ColumnMappings.Select(mapping =>
        {
            var columnName = mapping.Value;
            var property = mapping.Key;
            var dbType = _mapper.DbTypeMappings[property];
            var isKey = property == _mapper.KeyProperty;
            var isAutoIncrement = isKey && _mapper.IsAutoIncrement;

            var definition = $"{columnName} {dbType}";

            if (isKey)
            {
                definition += " PRIMARY KEY";
                if (isAutoIncrement)
                {
                    definition += " AUTOINCREMENT";
                }
            }
            else if (property.PropertyType == typeof(string) && !property.GetCustomAttributes(typeof(ColumnAttribute), false).Any())
            {
                // Default NOT NULL for strings without explicit attributes
                definition += " NOT NULL";
            }

            return definition;
        });

        sql.AppendLine(string.Join(",\n", columnDefinitions));
        sql.AppendLine(");");

        return sql.ToString();
    }

    public string GenerateDropTableSql()
    {
        return $"DROP TABLE IF EXISTS {_mapper.TableName};";
    }

    public string GenerateTableExistsSql()
    {
        return $"SELECT name FROM sqlite_master WHERE type='table' AND name='{_mapper.TableName}';";
    }
}
