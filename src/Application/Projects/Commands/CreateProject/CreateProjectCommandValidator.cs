using FluentValidation;

namespace Application.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator() {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.StartDate).GreaterThan(x => x.EndDate);
    }
}