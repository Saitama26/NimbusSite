using FluentValidation;

namespace Application.Projects.Commands.ChangeProjectStatus;

internal sealed class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

