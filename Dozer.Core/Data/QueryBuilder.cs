using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dozer.Core.Mapping;

namespace Dozer.Core.Data;

public class QueryBuilder<T> where T : class
{
    private readonly EntityMapper<T> _mapper;
    private readonly List<string> _whereConditions;
    private readonly List<string> _orderByClauses;
    private readonly List<QueryParameter> _parameters;
    private int? _limit;
    private int? _offset;

    public QueryBuilder(EntityMapper<T> mapper)
    {
        _mapper = mapper;
        _whereConditions = new List<string>();
        _orderByClauses = new List<string>();
        _parameters = new List<QueryParameter>();
    }

    public QueryBuilder<T> Where(string condition)
    {
        if (!string.IsNullOrWhiteSpace(condition))
        {
            _whereConditions.Add(condition);
        }
        return this;
    }

    public QueryBuilder<T> Where(string column, string op, object value)
    {
        var paramName = $"@param{_parameters.Count}";
        _whereConditions.Add($"{column} {op} {paramName}");
        _parameters.Add(new QueryParameter(paramName, value));
        return this;
    }

    public QueryBuilder<T> WhereEquals(string column, object value)
    {
        return Where(column, "=", value);
    }

    public QueryBuilder<T> WhereGreaterThan(string column, object value)
    {
        return Where(column, ">", value);
    }

    public QueryBuilder<T> WhereLessThan(string column, object value)
    {
        return Where(column, "<", value);
    }

    public QueryBuilder<T> WhereContains(string column, string value)
    {
        return Where(column, "LIKE", $"%{value}%");
    }

    public QueryBuilder<T> OrderBy(string column, bool ascending = true)
    {
        var direction = ascending ? "ASC" : "DESC";
        _orderByClauses.Add($"{column} {direction}");
        return this;
    }

    public QueryBuilder<T> Limit(int limit)
    {
        _limit = limit;
        return this;
    }

    public QueryBuilder<T> Offset(int offset)
    {
        _offset = offset;
        return this;
    }

    public string BuildSql()
    {
        var sql = new StringBuilder();
        sql.Append($"SELECT {string.Join(", ", _mapper.ColumnMappings.Values)} FROM {_mapper.TableName}");

        if (_whereConditions.Any())
        {
            sql.Append($" WHERE {string.Join(" AND ", _whereConditions)}");
        }

        if (_orderByClauses.Any())
        {
            sql.Append($" ORDER BY {string.Join(", ", _orderByClauses)}");
        }

        if (_limit.HasValue)
        {
            sql.Append($" LIMIT {_limit.Value}");
        }

        if (_offset.HasValue)
        {
            sql.Append($" OFFSET {_offset.Value}");
        }

        return sql.ToString();
    }

    public List<QueryParameter> GetParameters()
    {
        return _parameters.ToList();
    }

    public class QueryParameter
    {
        public string Name { get; }
        public object Value { get; }

        public QueryParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
