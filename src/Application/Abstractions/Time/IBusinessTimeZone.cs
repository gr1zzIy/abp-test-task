namespace Application.Abstractions.Time;

/// <summary>
/// Надає перетворення абсолютного часу в локальний час бізнесу,
/// який використовується для тарифікації та часових обмежень.
/// </summary>
public interface IBusinessTimeZone
{
    DateTimeOffset ConvertFromUtc(DateTimeOffset utcTime);
}