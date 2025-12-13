using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Users.Application.Abstractions;
using Users.Application.Commands.RegisterUser;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Contracts.Users;
using Users.Domain.Errors;
using Contracts.Users.Events;
using BCrypt.Net;

namespace Users.Application.Commands.RegisterUser;

/// <summary>
/// Команда регистрации нового пользователя
/// Создает User и публикует событие UserRegisteredEvent с хешем пароля
/// </summary>
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string Name,
    string? Phone = null,
    string? Bio = null) : ICommand<RegisterUserResponse>;

/// <summary>
/// Ответ при регистрации пользователя
/// </summary>
public sealed record RegisterUserResponse(Guid UserId);

/// <summary>
/// Обработчик команды регистрации пользователя
/// </summary>
internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private const int BcryptWorkFactor = 12;

    public RegisterUserCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Валидация email
        var emailLower = command.Email.ToLowerInvariant().Trim();
        if (string.IsNullOrWhiteSpace(emailLower))
        {
            return Result<RegisterUserResponse>.Failure(UserErrors.EmailEmpty);
        }

        if (!emailLower.Contains('@') || emailLower.Length < 5)
        {
            return Result<RegisterUserResponse>.Failure(UserErrors.InvalidEmailFormat);
        }

        // Проверка уникальности email (глобально)
        var exists = await _repository.ExistsByEmailAsync(emailLower, cancellationToken);
        if (exists)
        {
            return Result<RegisterUserResponse>.Failure(UserErrors.EmailAlreadyExists(emailLower));
        }

        // Валидация пароля
        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return Result<RegisterUserResponse>.Failure(UserErrors.PasswordEmpty);
        }

        if (command.Password.Length < 8)
        {
            return Result<RegisterUserResponse>.Failure(UserErrors.PasswordTooWeak);
        }

        // Проверка сложности пароля (минимум одна заглавная, одна строчная, одна цифра)
        if (!System.Text.RegularExpressions.Regex.IsMatch(command.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)"))
        {
            return Result<RegisterUserResponse>.Failure(UserErrors.PasswordTooWeak);
        }

        // Валидация имени
        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<RegisterUserResponse>.Failure(UserErrors.NameEmpty);
        }

        // Хеширование пароля
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, BcryptWorkFactor);

        // Создание пользователя (без тенанта)
        var user = new User
        {
            Email = emailLower,
            Name = name,
            Status = UserStatus.Active,
            Phone = command.Phone?.Trim(),
            Bio = command.Bio?.Trim(),
        };

        // Публикация событий
        var events = new List<IDomainEvent>
        {
            // UserCreatedEvent для обратной совместимости (создаст UserCredentials с пустым паролем)
            new UserCreatedEvent(
                user.Id,
                user.Email,
                user.Name,
                (UserStatusContract)(int)user.Status,
                user.Phone,
                user.Bio,
                user.CreatedAt),
            
            // UserRegisteredEvent с хешем пароля (обновит UserCredentials с паролем)
            new UserRegisteredEvent(
                user.Id,
                user.Email,
                user.Name,
                (UserStatusContract)(int)user.Status,
                passwordHash,
                user.Phone,
                user.Bio,
                user.CreatedAt)
        };

        // Сохранение
        await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация событий в очередь
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result<RegisterUserResponse>.Success(new RegisterUserResponse(user.Id));
    }
}

/// <summary>
/// Валидатор команды регистрации пользователя
/// </summary>
internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one digit.");

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

