using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository) : IRatingService
{
    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (rating is <= 0 or > 5)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = "Rating",
                    ErrorMessage = "Rating must be between 1 and 5"
                }
            ]);
        }

        var movieExists = await movieRepository.ExistsByIdAsync(movieId, cancellationToken);

        if (!movieExists)
        {
            return false;
        }

        return await ratingRepository.RateMovieAsync(movieId, rating, userId, cancellationToken);
    }
}