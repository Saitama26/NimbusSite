using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.Errors;
using Contracts.Projects;
using Contracts.Projects.Events;
using Tenants.Application.Abstractions;
using Tenants.Application.Commands.CreateTenant;
using Users.Domain.Enums;
using Users.Application.Abstractions;

namespace Projects.Application.Commands.CreateProject;

/// <summary>
/// Команда создания проекта.
/// Если у пользователя нет тенанта, он будет создан автоматически.
/// </summary>
/// <param name="UserId">Идентификатор пользователя, создающего проект</param>
/// <param name="Name">Название проекта (обязательное, должно быть уникальным в рамках тенанта)</param>
/// <param name="Description">Описание проекта (необязательное)</param>
/// <param name="TenantName">Название тенанта (используется только если тенант создается автоматически)</param>
public sealed record CreateProjectCommand(
    Guid UserId,
    string Name,
    string? Description = null,
    string? TenantName = null) : ICommand<CreateProjectResponse>;

/// <summary>
/// Ответ при создании проекта.
/// </summary>
/// <param name="ProjectId">Идентификатор созданного проекта</param>
public sealed record CreateProjectResponse(Guid ProjectId);

/// <summary>
/// Обработчик создания проекта.
/// </summary>
internal sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly IProjectRepository _repository;
    private readonly Projects.Application.Abstractions.IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ISender _sender;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly IUserRepository _userRepository;

    public CreateProjectCommandHandler(
        IProjectRepository repository,
        Projects.Application.Abstractions.IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ISender sender,
        IUserTenantRepository userTenantRepository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _sender = sender;
        _userTenantRepository = userTenantRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<CreateProjectResponse>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.NameEmpty);
        }

        // Проверяем существование пользователя
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null)
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.UserNotFound(command.UserId));
        }

        // Проверяем, есть ли у пользователя тенант, которым он владеет
        var hasOwnerTenant = await _userTenantRepository.HasOwnerTenantAsync(command.UserId, cancellationToken);
        Guid tenantId;

        if (!hasOwnerTenant)
        {
            // Создаем тенант автоматически
            var tenantName = !string.IsNullOrWhiteSpace(command.TenantName)
                ? command.TenantName.Trim()
                : $"{name} Tenant"; // Используем название проекта как название тенанта

            var createTenantCommand = new CreateTenantCommand(
                tenantName,
                command.UserId,
                null, // ConnectionString
                $"Tenant created automatically for project {name}");

            var createTenantResult = await _sender.Send<CreateTenantCommand, CreateTenantResponse>(createTenantCommand, cancellationToken);
            if (!createTenantResult.IsSuccess)
            {
                return Result<CreateProjectResponse>.Failure(createTenantResult.Error!);
            }

            tenantId = createTenantResult.Value!.TenantId;

            // UserTenant будет создан автоматически через обработчик события TenantCreatedEvent
            // Обработчик события создаст связь UserTenant с IsOwner = true
        }
        else
        {
            // Получаем тенант, которым владеет пользователь
            var ownerTenant = await _userTenantRepository.GetOwnerTenantByUserIdAsync(command.UserId, cancellationToken);
            if (ownerTenant == null)
            {
                return Result<CreateProjectResponse>.Failure(ProjectErrors.NotFound(Guid.Empty));
            }
            tenantId = ownerTenant.TenantId;
        }

        // Проверяем уникальность имени проекта в рамках тенанта
        var exists = await _repository.ExistsByNameAsync(tenantId, name, cancellationToken);
        if (exists)
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.NameAlreadyExists(name));
        }

        // Создаем проект
        var project = new Project
        {
            TenantId = tenantId,
            Name = name,
            Description = command.Description?.Trim(),
            Status = ProjectStatus.Active,
        };

        var events = new List<IDomainEvent>
        {
            new ProjectCreatedEvent(
                project.Id,
                project.TenantId,
                command.UserId,
                project.Name,
                (ProjectStatusContract)(int)project.Status,
                project.Description,
                project.CreatedAt)
        };

        await _repository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result<CreateProjectResponse>.Success(new CreateProjectResponse(project.Id));
    }
}

