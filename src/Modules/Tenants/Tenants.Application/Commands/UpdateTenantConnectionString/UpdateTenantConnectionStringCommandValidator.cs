using FluentValidation;

namespace Tenants.Application.Commands.UpdateTenantConnectionString;

/// <summary>
/// Валидатор команды обновления строки подключения
/// </summary>
internal sealed class UpdateTenantConnectionStringCommandValidator : AbstractValidator<UpdateTenantConnectionStringCommand>
{
    public UpdateTenantConnectionStringCommandValidator()
    {
        RuleFor(x => x.TenantInt)
            .GreaterThan(0).WithMessage("TenantInt must be greater than zero.");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string is required.")
            .MaximumLength(500).WithMessage("Connection string must not exceed 500 characters.");
    }
}

