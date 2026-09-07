using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request) => new()
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        YearOfRelease = request.YearOfRelease,
        Genres = [.. request.Genres]
    };

    public static Movie MapToMovie(this UpdateMovieRequest request, Guid id) => new()
    {
        Id = id,
        Title = request.Title,
        YearOfRelease = request.YearOfRelease,
        Genres = [.. request.Genres]
    };

    public static MovieResponse MapToMovieResponse(this Movie movie) => new()
    {
        Id = movie.Id,
        Title = movie.Title,
        YearOfRelease = movie.YearOfRelease,
        Genres = [.. movie.Genres]
    };

    public static MoviesResponse MapToMoviesResponse(this IEnumerable<Movie> movies) => new()
    {
        Items = movies.Select(MapToMovieResponse)
    };
}