using FluentValidation;

namespace Tasks.Application.Commands.CreateTask;

/// <summary>
/// Валидатор команды создания задачи
/// </summary>
internal sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be greater than 0.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(500).WithMessage("Task title must not exceed 500 characters.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Due date cannot be in the past.")
            .When(x => x.DueDate.HasValue);
    }
}

