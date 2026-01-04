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
        RuleFor(x => x.TenantInt)
            .GreaterThan(0).WithMessage("TenantInt must be greater than zero.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid tenant status.");
    }
}

