using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Validators;

namespace Movies.Application.Services;

public class MovieService(
    IMovieRepository movieRepository,
    IRatingRepository ratingRepository,
    MovieValidator movieValidator) : IMovieService
{
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await movieRepository.CreateAsync(movie, cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        return await movieRepository.GetByIdAsync(id, userId, cancellationToken);
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken = default)
    {
        return await movieRepository.GetBySlugAsync(slug, userId, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        return await movieRepository.GetAllAsync(userId, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId, CancellationToken cancellationToken = default)
    {
        await movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        var movieExists = await movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!movieExists)
        {
            return null;
        }

        await movieRepository.UpdateAsync(movie, cancellationToken);

        if (!userId.HasValue)
        {
            var rating = await ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;
            return movie;
        }

        var ratings = await ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
        movie.Rating = ratings.Rating;
        movie.UserRating = ratings.UserRating;

        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await movieRepository.DeleteByIdAsync(id, cancellationToken);
    }
}