using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok("Sup ma boi!");
    }
}