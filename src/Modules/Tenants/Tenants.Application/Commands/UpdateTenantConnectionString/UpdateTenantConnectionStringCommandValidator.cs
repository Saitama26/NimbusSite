using FluentValidation;

namespace Tenants.Application.Commands.UpdateTenantConnectionString;

/// <summary>
/// Валидатор команды обновления строки подключения
/// </summary>
internal sealed class UpdateTenantConnectionStringCommandValidator : AbstractValidator<UpdateTenantConnectionStringCommand>
{
    public UpdateTenantConnectionStringCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string is required.")
            .MaximumLength(500).WithMessage("Connection string must not exceed 500 characters.");
    }
}

