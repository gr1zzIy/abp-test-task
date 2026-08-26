using FluentValidation;

namespace Application.Reports;

public sealed class ReportPeriodValidator : AbstractValidator<ReportPeriod>
{
    public ReportPeriodValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty();

        RuleFor(x => x.To)
            .NotEmpty()
            .GreaterThan(x => x.From)
            .WithMessage("Кінець періоду повинен бути пізніше його початку.");
    }
}