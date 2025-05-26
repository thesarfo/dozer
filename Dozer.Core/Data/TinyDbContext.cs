using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dozer.Core.Mapping;

namespace Dozer.Core.Data;

public class TinyDbContext : IDisposable
{
    private readonly DbConnectionFactory _connectionFactory;
    private IDbConnection _connection;

    public TinyDbContext(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private IDbConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = _connectionFactory.CreateConnection();
                _connection.Open();
            }
            return _connection;
        }
    }

    public void Insert<T>(T entity) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);
        
        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateInsertSql(!mapper.IsAutoIncrement);

        AddParameters(cmd, entity, mapper);
        cmd.ExecuteNonQuery();
    }

    public void Update<T>(T entity) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateUpdateSql();

        AddParameters(cmd, entity, mapper);
        cmd.ExecuteNonQuery();
    }

    public void Delete<T>(T entity) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateDeleteSql();

        var keyColumn = mapper.ColumnMappings[mapper.KeyProperty];
        var param = _connectionFactory.CreateParameter();
        param.ParameterName = $"@{keyColumn}";
        param.Value = mapper.KeyProperty.GetValue(entity) ?? DBNull.Value;
        cmd.Parameters.Add(param);

        cmd.ExecuteNonQuery();
    }

    public List<T> List<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateSelectAllSql();

        using var reader = cmd.ExecuteReader();
        return MapResults<T>(reader, mapper).ToList();
    }

    private void AddParameters<T>(IDbCommand cmd, T entity, EntityMapper<T> mapper) where T : class
    {
        foreach (var mapping in mapper.ColumnMappings)
        {
            var param = _connectionFactory.CreateParameter();
            param.ParameterName = $"@{mapping.Value}";
            param.Value = mapping.Key.GetValue(entity) ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }
    }

    private IEnumerable<T> MapResults<T>(IDataReader reader, EntityMapper<T> mapper) where T : class
    {
        var type = typeof(T);
        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
        {
            throw new InvalidOperationException($"Type {type.Name} must have a parameterless constructor");
        }

        var columnMap = mapper.ColumnMappings
            .ToDictionary(x => x.Value.ToLower(), x => x.Key);

        while (reader.Read())
        {
            var instance = (T)constructor.Invoke(null);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i).ToLower();
                if (columnMap.TryGetValue(columnName, out PropertyInfo property))
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    if (value != null)
                    {
                        property.SetValue(instance, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }

            yield return instance;
        }
    }

    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Dispose();
            _connection = null;
        }
    }
} 