using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class RatingRepository(IDbConnectionFactory dbConnectionFactory) : IRatingRepository
{
    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO ratings(userid, movieid, rating)
            VALUES (@userId, @movieId, @rating)
            ON CONFLICT (userid, movieid) DO UPDATE 
            SET rating = @rating
            """, new { userId, movieId, rating }, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<float?>(
            new CommandDefinition("SELECT ROUND(AVG(r.rating), 1) FROM ratings r WHERE movieid = @movieId",
                new { movieId }, cancellationToken: cancellationToken));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(
            new CommandDefinition(
                """
                SELECT ROUND(AVG(r.rating), 1), 
                    (SELECT rating 
                     FROM ratings 
                     WHERE movieid = @movieId AND userid = @userId 
                     LIMIT 1) 
                FROM ratings r 
                WHERE movieid = @movieId
                """,
                new { movieId, userId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            DELETE FROM ratings
            WHERE movieid = @movieId AND userid = @userId
            """, new { userId, movieId }, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<MovieRating>(new CommandDefinition(
            """
            SELECT r.rating, r.movieid, m.slug
            FROM ratings r
            INNER JOIN movies m on r.movieid = m.id
            WHERE userid = @userId
            """, new { userId }, cancellationToken: cancellationToken));
    }
}