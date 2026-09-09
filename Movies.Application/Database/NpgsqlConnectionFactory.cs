using System.Data;
using Npgsql;

namespace Movies.Application.Database;

public class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}