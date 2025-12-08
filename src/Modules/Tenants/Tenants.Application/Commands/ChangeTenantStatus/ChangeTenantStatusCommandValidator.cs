using FluentValidation;
using Tenants.Application.Commands.ChangeTenantStatus;
using Tenants.Domain.Enums;

namespace Tenants.Application.Commands.ChangeTenantStatus;

/// <summary>
/// Валидатор команды изменения статуса тенанта
/// </summary>
internal sealed class ChangeTenantStatusCommandValidator : AbstractValidator<ChangeTenantStatusCommand>
{
    public ChangeTenantStatusCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid tenant status.");
    }
}

