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
	}
}