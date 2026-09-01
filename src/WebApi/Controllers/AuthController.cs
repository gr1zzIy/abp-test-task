using Application.Authentication.Login;
using Application.Authentication.Me;
using Application.Authentication.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Contracts.Authentication;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// Реєструє нового клієнта.
    /// </summary>
    /// <remarks>
    /// Через публічну реєстрацію створюються лише користувачі
    /// з роллю Client.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<RegisterResult>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResult>> Register(
        RegisterRequest request,
        [FromServices] RegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RegisterCommand(
                request.Email,
                request.Password),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    /// <summary>
    /// Автентифікує користувача та повертає JWT access token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResult>> Login(
        LoginRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new LoginCommand(
                request.Email,
                request.Password),
            cancellationToken);

        return Ok(result);
    }
    
    /// <summary>
    /// Повертає дані поточного автентифікованого користувача.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResult> Me(
        [FromServices] GetCurrentUserHandler handler)
    {
        var result = handler.Handle();

        return Ok(result);
    }
}