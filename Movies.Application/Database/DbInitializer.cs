using Dapper;

namespace Movies.Application.Database;

public class DbInitializer(IDbConnectionFactory dbConnectionFactory)
{
    public async Task InitializeAsync()
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("""
                                          CREATE TABLE IF NOT EXISTS movies (
                                          id UUID PRIMARY KEY,
                                          slug TEXT NOT NULL,
                                          title TEXT NOT NULL,
                                          yearOfRelease INTEGER NOT NULL);   
                                      """);

        await connection.ExecuteAsync("""
                                        CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS movies_slug_index
                                        ON movies
                                        USING BTREE(slug)
                                      """);

        await connection.ExecuteAsync("""
                                        CREATE TABLE IF NOT EXISTS genres ( 
                                        movieId UUID REFERENCES movies (Id),
                                        name TEXT NOT NULL);
                                      """);
    }
}