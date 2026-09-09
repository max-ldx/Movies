using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieService movieService) : ControllerBase
{
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();

        await movieService.CreateAsync(movie);

        var response = movie.MapToMovieResponse();

        return CreatedAtAction(nameof(Get), new { idOrSlug = response.Id }, response);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await movieService.GetByIdAsync(id)
            : await movieService.GetBySlugAsync(idOrSlug);

        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToMovieResponse();

        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await movieService.GetAllAsync();

        var response = movies.MapToMoviesResponse();

        return Ok(response);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);

        var updatedMovie = await movieService.UpdateAsync(movie);

        if (updatedMovie is null)
        {
            return NotFound();
        }

        var response = movie.MapToMovieResponse();

        return Ok(response);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await movieService.DeleteByIdAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}