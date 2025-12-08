using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Tasks;
using Domain.Tasks.Errors;
using Domain.Tasks.Events;
using SharedKernel;

namespace Application.Tasks.Commands.AssignTask;

internal sealed class AssignTaskCommandHandler : ICommandHandler<AssignTaskCommand, Guid>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IApplicationDbContext _context;

    public AssignTaskCommandHandler(ITaskRepository taskRepository, IApplicationDbContext context)
    {
        _taskRepository = taskRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AssignTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

        if (task is null)
        {
            return TaskErrors.NotFound(command.TaskId);
        }

        if (task.AssignedUserId == command.AssignedUserId)
        {
            return task.Id; // Уже назначена на этого пользователя
        }

        var oldAssigneeId = task.AssignedUserId;
        task.AssignedUserId = command.AssignedUserId;
        task.AddEvent(new TaskAssignedEvent(task.Id, command.AssignedUserId));

        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}

