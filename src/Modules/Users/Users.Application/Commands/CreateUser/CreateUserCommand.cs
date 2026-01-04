using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Contracts.Events;
using Users.Contracts.Enums;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Errors;

namespace Users.Application.Commands.CreateUser;

/// <summary>
/// Команда создания нового пользователя
/// Пользователь создается в tenant-специфичной БД со схемой Users
/// </summary>
public sealed record CreateUserCommand(
    int TenantId,
    string Email,
    string Name,
    string? Phone = null,
    string? Bio = null) : ICommand<CreateUserResponse>;

/// <summary>
/// Ответ при создании пользователя
/// </summary>
public sealed record CreateUserResponse(Guid UserId);

/// <summary>
/// Обработчик команды создания пользователя
/// </summary>
internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUsersDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public CreateUserCommandHandler(
        IUsersDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Валидация email
        var emailLower = command.Email.ToLowerInvariant().Trim();
        if (string.IsNullOrWhiteSpace(emailLower))
        {
            return Result<CreateUserResponse>.Failure(UserErrors.EmailEmpty);
        }

        if (!emailLower.Contains('@') || emailLower.Length < 5)
        {
            return Result<CreateUserResponse>.Failure(UserErrors.InvalidEmailFormat);
        }

        // Проверка уникальности email в рамках тенанта
        var exists = await _dbContext.Users
            .AnyAsync(u => u.TenantId == command.TenantId && u.Email == emailLower, cancellationToken);
        if (exists)
        {
            return Result<CreateUserResponse>.Failure(UserErrors.EmailAlreadyExists(emailLower));
        }

        // Валидация имени
        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<CreateUserResponse>.Failure(UserErrors.NameEmpty);
        }

        // Создание пользователя
        var user = new User
        {
            TenantId = command.TenantId,
            Email = emailLower,
            Name = name,
            Status = UserStatus.Active,
            Phone = command.Phone?.Trim(),
            Bio = command.Bio?.Trim(),
        };

        _dbContext.Users.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new UserCreatedEvent(
            user.Id,
            user.TenantId,
            user.Email,
            user.Name,
            (UserStatusContract)(int)user.Status,
            user.Phone,
            user.Bio,
            user.CreatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result<CreateUserResponse>.Success(new CreateUserResponse(user.Id));
    }
}

/// <summary>
/// Валидатор команды создания пользователя
/// </summary>
internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("User name is required.")
            .MaximumLength(200).WithMessage("User name must not exceed 200 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));
    }
}

