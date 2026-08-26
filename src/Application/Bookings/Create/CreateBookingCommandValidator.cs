using FluentValidation;

namespace Application.Bookings.Create;

public sealed class CreateBookingCommandValidator
		: AbstractValidator<CreateBookingCommand>
{
	public CreateBookingCommandValidator()
	{
		RuleFor(x => x.ConferenceRoomId)
				.NotEmpty();

		RuleFor(x => x.StartTime)
				.NotEmpty();

		RuleFor(x => x.EndTime)
				.NotEmpty()
				.GreaterThan(x => x.StartTime)
				.WithMessage("Час завершення повинен бути пізніше часу початку.");

		RuleFor(x => x.ServiceIds)
				.NotNull();

		RuleFor(x => x)
				.Must(BeWithinWorkingHours)
				.WithMessage(
				"Бронювання повинно бути в межах 06:00–23:00 одного календарного дня.");
	}

	private static bool BeWithinWorkingHours(
			CreateBookingCommand command)
	{
		if (command.StartTime.Date != command.EndTime.Date)
		{
			return false;
		}

		var start = command.StartTime.TimeOfDay;
		var end = command.EndTime.TimeOfDay;

		return start >= TimeSpan.FromHours(6) &&
		       end <= TimeSpan.FromHours(23);
	}
}