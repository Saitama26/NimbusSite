using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Users.Application.Abstractions;
using Users.Application.Commands.CreateUser;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Contracts.Users;
using Users.Domain.Errors;
using Contracts.Users.Events;

namespace Users.Application.Commands.CreateUser;

/// <summary>
/// Команда создания нового пользователя
/// Пользователь создается без тенанта, тенант добавляется позже через UserTenant
/// </summary>
public sealed record CreateUserCommand(
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
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public CreateUserCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
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

        // Проверка уникальности email (глобально)
        var exists = await _repository.ExistsByEmailAsync(emailLower, cancellationToken);
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

        // Создание пользователя (без тенанта)
        var user = new User
        {
            Email = emailLower,
            Name = name,
            Status = UserStatus.Active,
            Phone = command.Phone?.Trim(),
            Bio = command.Bio?.Trim(),
        };

        var events = new List<IDomainEvent>
        {
            new UserCreatedEvent(
                user.Id,
                user.Email,
                user.Name,
                (UserStatusContract)(int)user.Status,
                user.Phone,
                user.Bio,
                user.CreatedAt)
        };

        // Сохранение
        await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация события в очередь
        await _eventBus.PublishAsync(events, cancellationToken);

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

