using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO movies (id, slug, title, yearofrelease)
            values (@Id, @Slug, @Title, @YearOfRelease)
            """, movie, cancellationToken: cancellationToken));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO genres (movieid, name)
                    VALUES (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre }, cancellationToken: cancellationToken));
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT m.*, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS userrating
                                  FROM movies m 
                                  LEFT JOIN ratings r ON m.id = r.movieid
                                  LEFT JOIN ratings my on m.id = myr.movieid AND myr.userid = @userid
                                  WHERE id = @id
                                  GROUP BY id, userrating
                                  """, new { id, userId },
                cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("SELECT name FROM genres WHERE movieid = @id", new { id },
                cancellationToken: cancellationToken));

        movie.Genres.AddRange(genres);

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QueryFirstOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT m.*, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS userrating
                                  FROM movies m 
                                  LEFT JOIN ratings r ON m.id = r.movieid
                                  LEFT JOIN ratings my on m.id = myr.movieid AND myr.userid = @userid
                                  WHERE slug = @slug
                                  GROUP BY id, userrating
                                  """, new { slug, userId },
                cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("SELECT name FROM genres WHERE movieid = @id", new { movie.Id },
                cancellationToken: cancellationToken));

        movie.Genres.AddRange(genres);

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync(new CommandDefinition(
            """
            SELECT m.*, string_agg(DISTINCT g.name, ',') AS genres, ROUND(AVG(r.rating), 1) AS rating, myr.rating AS userrating
            FROM movies m 
            LEFT JOIN genres g ON m.id = g.movieid
            LEFT JOIN ratings r ON m.id = r.movieid
            LEFT JOIN ratings my on m.id = myr.movieid AND myr.userid = @userId
            GROUP BY id
            """, new { userId }, cancellationToken: cancellationToken));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userrating,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("DELETE FROM genres WHERE movieid = @id",
            new { id = movie.Id }));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO genres (movieid, name)
                VALUES (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: cancellationToken));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE movies SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease WHERE id = @Id", movie,
            cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("DELETE FROM genres WHERE movieid = @id",
            new { id }, cancellationToken: cancellationToken));

        var result =
            await connection.ExecuteAsync(new CommandDefinition("DELETE FROM movies WHERE id = @id", new { id },
                cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("SELECT COUNT(1) FROM movies WHERE id = @id", new { id }));
    }
}