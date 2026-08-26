using Application.Abstractions.Persistence;

namespace Application.Services.GetAll;

/// <summary>
/// Повертає перелік додаткових послуг,
/// які можна використовувати під час налаштування конференц-залу.
/// </summary>
public sealed class GetServicesHandler
{
	private readonly IServiceRepository _serviceRepository;

	public GetServicesHandler(IServiceRepository serviceRepository)
	{
		_serviceRepository = serviceRepository;
	}

	public async Task<IReadOnlyCollection<ServiceResult>> HandleAsync(
			CancellationToken cancellationToken = default)
	{
		var services = await _serviceRepository.GetAllAsync(
		cancellationToken);

		return services
				.Select(service => new ServiceResult(
				service.Id,
				service.Name,
				service.Price))
				.ToArray();
	}
}