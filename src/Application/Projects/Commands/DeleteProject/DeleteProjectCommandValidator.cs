using FluentValidation;

namespace Application.Projects.Commands.DeleteProject;

internal sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}

