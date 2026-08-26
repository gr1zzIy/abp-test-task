using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    /// <summary>
    /// Перевіряє доступність Web API.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok("Sup ma boi!");
    }
}