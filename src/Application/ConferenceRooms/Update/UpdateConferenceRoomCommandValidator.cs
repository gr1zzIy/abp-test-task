using FluentValidation;

namespace Application.ConferenceRooms.Update;

public sealed class UpdateConferenceRoomCommandValidator
    : AbstractValidator<UpdateConferenceRoomCommand>
{
    public UpdateConferenceRoomCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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