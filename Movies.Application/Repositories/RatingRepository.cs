using Dapper;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class RatingRepository(IDbConnectionFactory dbConnectionFactory) : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<float?>(
            new CommandDefinition("SELECT ROUND(AVG(r.rating), 1) FROM ratings r WHERE movieid = @movieId",
                new { movieId }, cancellationToken: cancellationToken));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
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
}