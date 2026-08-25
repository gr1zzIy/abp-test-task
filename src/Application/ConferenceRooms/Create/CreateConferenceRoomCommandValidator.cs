using FluentValidation;

namespace Application.ConferenceRooms.Create;

public sealed class CreateConferenceRoomCommandValidator
    : AbstractValidator<CreateConferenceRoomCommand>
{
    public CreateConferenceRoomCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Capacity)
            .GreaterThan(0);

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0);

        RuleFor(x => x.ServiceIds)
            .NotNull();
    }
}