using System.Data;
using System.Data.Common;

namespace Dozer.Core.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;
    private readonly DbProviderFactory _providerFactory;

    public DbConnectionFactory(string connectionString, DbProviderFactory providerFactory)
    {
        _connectionString = connectionString;
        _providerFactory = providerFactory;
    }

    public IDbConnection CreateConnection()
    {
        var connection = _providerFactory.CreateConnection();
        if (connection == null)
        {
            throw new DataException("Failed to create database connection");
        }

        connection.ConnectionString = _connectionString;
        return connection;
    }

    public IDbCommand CreateCommand()
    {
        var command = _providerFactory.CreateCommand();
        if (command == null)
        {
            throw new DataException("Failed to create database command");
        }
        return command;
    }

    public IDbDataParameter CreateParameter()
    {
        var parameter = _providerFactory.CreateParameter();
        if (parameter == null)
        {
            throw new DataException("Failed to create database parameter");
        }
        return parameter;
    }
} 