using FluentValidation;

namespace Tenants.Application.Commands.DeleteTenant;

/// <summary>
/// Валидатор команды удаления тенанта
/// </summary>
internal sealed class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");
    }
}

