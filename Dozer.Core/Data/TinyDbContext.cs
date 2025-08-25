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
    private readonly Dictionary<Type, object> _entityTrackers;

    public TinyDbContext(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _entityTrackers = new Dictionary<Type, object>();
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
        
        // If its an autoincrement pk, we get the generated ID and set it back to the entity
        if (mapper.IsAutoIncrement && mapper.KeyProperty != null)
        {
            cmd.CommandText = "SELECT last_insert_rowid()";
            var generatedId = cmd.ExecuteScalar();
            if (generatedId != null)
            {
                mapper.KeyProperty.SetValue(entity, Convert.ChangeType(generatedId, mapper.KeyProperty.PropertyType));
            }
        }

        // after a successful insert, we track the entity
        GetEntityTracker<T>().TrackEntity(entity);
    }

    public void Update<T>(T entity) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);
        var tracker = GetEntityTracker<T>();

        // we check if entity is tracked and has changes
        if (tracker.IsModified(entity))
        {
            var modifiedProperties = tracker.GetModifiedProperties(entity);
            if (modifiedProperties.Any())
            {
                // then we only update modified properties
                using var cmd = _connectionFactory.CreateCommand();
                cmd.Connection = Connection;
                cmd.CommandText = sqlGenerator.GenerateUpdateSql(modifiedProperties);

                AddModifiedParameters(cmd, entity, mapper, modifiedProperties);
                cmd.ExecuteNonQuery();
            }
        }
        else
        {
            using var cmd = _connectionFactory.CreateCommand();
            cmd.Connection = Connection;
            cmd.CommandText = sqlGenerator.GenerateUpdateSql();

            AddParameters(cmd, entity, mapper);
            cmd.ExecuteNonQuery();
        }

        tracker.AcceptChanges(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);
        var tracker = GetEntityTracker<T>();

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateDeleteSql();

        var keyColumn = mapper.ColumnMappings[mapper.KeyProperty];
        var param = _connectionFactory.CreateParameter();
        param.ParameterName = $"@{keyColumn}";
        param.Value = mapper.KeyProperty.GetValue(entity) ?? DBNull.Value;
        cmd.Parameters.Add(param);

        cmd.ExecuteNonQuery();

        // mark it as deleted in tracker
        tracker.MarkAsDeleted(entity);
    }

    public List<T> List<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);
        var tracker = GetEntityTracker<T>();

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateSelectAllSql();

        using var reader = cmd.ExecuteReader();
        var results = MapResults<T>(reader, mapper).ToList();
        
        // track loaded entities
        foreach (var entity in results)
        {
            tracker.TrackEntity(entity);
        }
        
        return results;
    }

    public T FindById<T>(object id) where T : class
    {
        var mapper = new EntityMapper<T>();
        var sqlGenerator = new SqlGenerator<T>(mapper);
        var tracker = GetEntityTracker<T>();

        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = sqlGenerator.GenerateSelectByIdSql();

        var keyColumn = mapper.ColumnMappings[mapper.KeyProperty];
        var param = _connectionFactory.CreateParameter();
        param.ParameterName = $"@{keyColumn}";
        param.Value = id ?? DBNull.Value;
        cmd.Parameters.Add(param);

        using var reader = cmd.ExecuteReader();
        var result = MapResults<T>(reader, mapper).FirstOrDefault();
        
        if (result != null)
        {
            tracker.TrackEntity(result);
        }
        
        return result;
    }

    public QueryBuilder<T> Query<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        return new QueryBuilder<T>(mapper);
    }

    public List<T> ExecuteQuery<T>(QueryBuilder<T> queryBuilder) where T : class
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

        using var reader = cmd.ExecuteReader();
        return MapResults<T>(reader, mapper).ToList();
    }

    public T FirstOrDefault<T>(QueryBuilder<T> queryBuilder) where T : class
    {
        var results = ExecuteQuery(queryBuilder.Limit(1));
        return results.FirstOrDefault();
    }

    public int Count<T>(QueryBuilder<T> queryBuilder) where T : class
    {
        var originalSql = queryBuilder.BuildSql();
        var countSql = originalSql.Replace("SELECT " + string.Join(", ", new EntityMapper<T>().ColumnMappings.Values), "SELECT COUNT(*)");
        
        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = countSql;

        // Add parameters
        foreach (var param in queryBuilder.GetParameters())
        {
            var dbParam = _connectionFactory.CreateParameter();
            dbParam.ParameterName = param.Name;
            dbParam.Value = param.Value ?? DBNull.Value;
            cmd.Parameters.Add(dbParam);
        }

        var result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public void EnsureTableExists<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        var schemaGenerator = new SchemaGenerator<T>(mapper);
        
        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = schemaGenerator.GenerateCreateTableSql();
        cmd.ExecuteNonQuery();
    }

    public bool TableExists<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        var schemaGenerator = new SchemaGenerator<T>(mapper);
        
        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = schemaGenerator.GenerateTableExistsSql();
        
        var result = cmd.ExecuteScalar();
        return result != null;
    }

    public void DropTable<T>() where T : class
    {
        var mapper = new EntityMapper<T>();
        var schemaGenerator = new SchemaGenerator<T>(mapper);
        
        using var cmd = _connectionFactory.CreateCommand();
        cmd.Connection = Connection;
        cmd.CommandText = schemaGenerator.GenerateDropTableSql();
        cmd.ExecuteNonQuery();
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

    public IDbTransaction BeginTransaction()
    {
        return Connection.BeginTransaction();
    }

    public void CommitTransaction(IDbTransaction transaction)
    {
        transaction?.Commit();
        transaction?.Dispose();
    }

    public void RollbackTransaction(IDbTransaction transaction)
    {
        transaction?.Rollback();
        transaction?.Dispose();
    }

    private EntityTracker<T> GetEntityTracker<T>() where T : class
    {
        var type = typeof(T);
        if (!_entityTrackers.TryGetValue(type, out var tracker))
        {
            tracker = new EntityTracker<T>();
            _entityTrackers[type] = tracker;
        }
        return (EntityTracker<T>)tracker;
    }

    private void AddModifiedParameters<T>(IDbCommand cmd, T entity, EntityMapper<T> mapper, Dictionary<PropertyInfo, object> modifiedProperties) where T : class
    {
        // add modified property parameters
        foreach (var kvp in modifiedProperties)
        {
            var property = kvp.Key;
            var value = kvp.Value;
            var columnName = mapper.ColumnMappings[property];
            
            var param = _connectionFactory.CreateParameter();
            param.ParameterName = $"@{columnName}";
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }

        // for WHERE clauses, we need to add the key parameter
        var keyColumn = mapper.ColumnMappings[mapper.KeyProperty];
        var keyParam = _connectionFactory.CreateParameter();
        keyParam.ParameterName = $"@{keyColumn}";
        keyParam.Value = mapper.KeyProperty.GetValue(entity) ?? DBNull.Value;
        cmd.Parameters.Add(keyParam);
    }

    public void TrackEntity<T>(T entity) where T : class
    {
        GetEntityTracker<T>().TrackEntity(entity);
    }

    public void MarkAsModified<T>(T entity) where T : class
    {
        GetEntityTracker<T>().MarkAsModified(entity);
    }

    public EntityState GetEntityState<T>(T entity) where T : class
    {
        return GetEntityTracker<T>().GetEntityState(entity);
    }

    public void AcceptChanges<T>(T entity) where T : class
    {
        GetEntityTracker<T>().AcceptChanges(entity);
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