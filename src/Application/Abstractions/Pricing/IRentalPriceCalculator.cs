namespace Application.Abstractions.Pricing;

/// <summary>
/// Виконує розрахунок вартості оренди конференц-залу
/// відповідно до часових тарифів.
/// </summary>
public interface IRentalPriceCalculator
{
	decimal Calculate(
			decimal hourlyRate,
			DateTimeOffset startTime,
			DateTimeOffset endTime);
}