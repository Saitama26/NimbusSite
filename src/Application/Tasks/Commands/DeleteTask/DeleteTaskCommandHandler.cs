using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Tasks;
using Domain.Tasks.Errors;
using SharedKernel;

namespace Application.Tasks.Commands.DeleteTask;

internal sealed class DeleteTaskCommandHandler : ICommandHandler<DeleteTaskCommand, Guid>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IApplicationDbContext _context;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository, IApplicationDbContext context)
    {
        _taskRepository = taskRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

        if (task is null)
        {
            return TaskErrors.NotFound(command.TaskId);
        }

        _taskRepository.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        return command.TaskId;
    }
}

