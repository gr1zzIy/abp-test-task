using FluentValidation;

namespace Application.ConferenceRooms.SearchAvailable;

public sealed class SearchAvailableConferenceRoomsQueryValidator
		: AbstractValidator<SearchAvailableConferenceRoomsQuery>
{
	public SearchAvailableConferenceRoomsQueryValidator()
	{
		RuleFor(x => x.StartTime)
				.NotEmpty();

		RuleFor(x => x.EndTime)
				.NotEmpty()
				.GreaterThan(x => x.StartTime)
				.WithMessage("Час завершення повинен бути пізніше часу початку.");

		RuleFor(x => x.Capacity)
				.GreaterThan(0);
	}
}