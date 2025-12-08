using FluentValidation;

namespace Application.Tasks.Commands.UpdateTask;

internal sealed class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);
        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);
        RuleFor(x => x.DueDate)
            .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
            .WithMessage("Due date cannot be in the past")
            .When(x => x.DueDate.HasValue);
    }
}

