using System;
using System.Linq;
using System.Text;
using Dozer.Core.Mapping;

namespace Dozer.Core.Data;

public class SqlGenerator<T> where T : class
{
    private readonly EntityMapper<T> _mapper;

    public SqlGenerator(EntityMapper<T> mapper)
    {
        _mapper = mapper;
    }

    public string GenerateInsertSql(bool includeIdentity = true)
    {
        var columns = _mapper.ColumnMappings
            .Where(x => includeIdentity || x.Key != _mapper.KeyProperty)
            .Select(x => x.Value);

        var parameters = _mapper.ColumnMappings
            .Where(x => includeIdentity || x.Key != _mapper.KeyProperty)
            .Select(x => $"@{x.Value}");

        return $"INSERT INTO {_mapper.TableName} ({string.Join(", ", columns)}) " +
               $"VALUES ({string.Join(", ", parameters)})";
    }

    public string GenerateUpdateSql()
    {
        if (_mapper.KeyProperty == null)
            throw new InvalidOperationException("Entity must have a key property for update operations");

        var setColumns = _mapper.ColumnMappings
            .Where(x => x.Key != _mapper.KeyProperty)
            .Select(x => $"{x.Value} = @{x.Value}");

        var keyColumn = _mapper.ColumnMappings[_mapper.KeyProperty];
        
        return $"UPDATE {_mapper.TableName} " +
               $"SET {string.Join(", ", setColumns)} " +
               $"WHERE {keyColumn} = @{keyColumn}";
    }

    public string GenerateDeleteSql()
    {
        if (_mapper.KeyProperty == null)
            throw new InvalidOperationException("Entity must have a key property for delete operations");

        var keyColumn = _mapper.ColumnMappings[_mapper.KeyProperty];
        return $"DELETE FROM {_mapper.TableName} WHERE {keyColumn} = @{keyColumn}";
    }

    public string GenerateSelectAllSql()
    {
        return $"SELECT {string.Join(", ", _mapper.ColumnMappings.Values)} FROM {_mapper.TableName}";
    }

    public string GenerateSelectByIdSql()
    {
        if (_mapper.KeyProperty == null)
            throw new InvalidOperationException("Entity must have a key property for select by id operations");

        var keyColumn = _mapper.ColumnMappings[_mapper.KeyProperty];
        return $"SELECT {string.Join(", ", _mapper.ColumnMappings.Values)} " +
               $"FROM {_mapper.TableName} WHERE {keyColumn} = @{keyColumn}";
    }
} 