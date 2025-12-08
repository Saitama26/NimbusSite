using FluentValidation;

namespace Application.Tasks.Commands.AssignTask;

internal sealed class AssignTaskCommandValidator : AbstractValidator<AssignTaskCommand>
{
    public AssignTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.AssignedUserId).NotEmpty();
    }
}

