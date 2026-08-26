using Application.Services.GetAll;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/services")]
public sealed class ServicesController : ControllerBase
{
	/// <summary>
	/// Повертає список доступних додаткових послуг.
	/// </summary>
	/// <returns>
	/// Перелік послуг із їх ідентифікаторами, назвами та вартістю.
	/// </returns>
	[HttpGet]
	[ProducesResponseType<IReadOnlyCollection<ServiceResult>>(
		StatusCodes.Status200OK)]
	public async Task<ActionResult<IReadOnlyCollection<ServiceResult>>> GetAll(
		[FromServices] GetServicesHandler handler,
		CancellationToken cancellationToken)
	{
		var result = await handler.HandleAsync(cancellationToken);

		return Ok(result);
	}
}