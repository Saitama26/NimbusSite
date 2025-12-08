using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Tasks;
using Domain.Tasks.Errors;
using Domain.Tasks.ValueObjects;
using SharedKernel;

namespace Application.Tasks.Commands.UpdateTask;

internal sealed class UpdateTaskCommandHandler : ICommandHandler<UpdateTaskCommand, Guid>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IApplicationDbContext _context;

    public UpdateTaskCommandHandler(ITaskRepository taskRepository, IApplicationDbContext context)
    {
        _taskRepository = taskRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

        if (task is null)
        {
            return TaskErrors.NotFound(command.TaskId);
        }

        if (task.Status == Status.Done)
        {
            return TaskErrors.AlreadyCompleted();
        }

        if (command.DueDate.HasValue && command.DueDate.Value < DateTime.UtcNow)
        {
            return TaskErrors.DeadlineInPast();
        }

        if (command.Title is not null)
        {
            task.Title = command.Title;
        }

        if (command.Description is not null)
        {
            task.Description = command.Description;
        }

        if (command.DueDate.HasValue)
        {
            task.DueDate = command.DueDate;
        }

        if (command.Priority.HasValue)
        {
            task.Priority = command.Priority.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}

