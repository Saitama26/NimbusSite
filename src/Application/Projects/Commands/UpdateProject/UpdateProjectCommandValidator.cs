using FluentValidation;

namespace Application.Projects.Commands.UpdateProject;

internal sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => x.Name is not null);
        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.EndDate.Value >= x.StartDate.Value)
            .WithMessage("End date must be after start date")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

