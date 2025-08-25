using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dozer.Core.Mapping;

namespace Dozer.Core.Data;

public class AsyncTinyDbContext : IDisposable
{
    private readonly DbConnectionFactory _connectionFactory;
    private IDbConnection _connection;

    public AsyncTinyDbContext(DbConnectionFactory connectionFactory)
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

    public async Task InsertAsync<T>(T entity) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);
        
        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateInsertSql(!mapper.IsAutoIncrement);

        AddParameters(cmd, entity, mapper);
        await Task.Run(() => cmd.ExecuteNonQuery());
        
        // If its an autoincrement pk get the generated ID and set it back to the entity
        if (mapper.IsAutoIncrement && mapper.KeyProperty != null)
        {
            cmd.CommandText = "SELECT last_insert_rowid()";
            var generatedId = await Task.Run(() => cmd.ExecuteScalar());
            if (generatedId != null)
            {
                mapper.KeyProperty.SetValue(entity, Convert.ChangeType(generatedId, mapper.KeyProperty.PropertyType));
            }
        }
    }

    public async Task UpdateAsync<T>(T entity) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateUpdateSql();

        AddParameters(cmd, entity, mapper);
        await Task.Run(() => cmd.ExecuteNonQuery());
    }

    public async Task DeleteAsync<T>(T entity) where T : class
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

        await Task.Run(() => cmd.ExecuteNonQuery());
    }

    public async Task<List<T>> ListAsync<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateSelectAllSql();

        using var reader = await Task.Run(() => cmd.ExecuteReader());
        return MapResults<T>(reader, mapper).ToList();
    }

    public async Task<T> FindByIdAsync<T>(object id) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateSelectByIdSql();

        var keyColumn = mapper.ColumnMappings[mapper.KeyProperty];
        var param = _connectionFactory.CreateParameter();
        param.ParameterName = $"@{keyColumn}";
        param.Value = id ?? DBNull.Value;
        cmd.Parameters.Add(param);

        using var reader = await Task.Run(() => cmd.ExecuteReader());
        return MapResults<T>(reader, mapper).FirstOrDefault();
    }

    public QueryBuilder<T> Query<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        return new QueryBuilder<T>(mapper);
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(QueryBuilder<T> queryBuilder) where T : class
    {
        var mapper = new EntityMapper<T>();
        
        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = queryBuilder.BuildSql();

        // Add parameters
        foreach (var param in queryBuilder.GetParameters())
        {
            var dbParam = _connectionFactory.CreateParameter();
            dbParam.ParameterName = param.Name;
            dbParam.Value = param.Value ?? DBNull.Value;
            cmd.Parameters.Add(dbParam);
        }

        using var reader = await Task.Run(() => cmd.ExecuteReader());
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
