using BookIt.Api.Models;
using BookIt.Application;
using BookIt.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(ILibraryFacade library) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthenticationResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await library.RegisterMemberAsync(
            new RegisterMemberCommand(request.Name, request.Email, request.Password),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, AuthenticationResponse.FromResult(result));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await library.LoginAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Ok(AuthenticationResponse.FromResult(result));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<AuthenticationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Refresh(
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await library.RefreshSessionAsync(
            new RefreshSessionCommand(request.RefreshToken),
            cancellationToken);

        return Ok(AuthenticationResponse.FromResult(result));
    }
}
