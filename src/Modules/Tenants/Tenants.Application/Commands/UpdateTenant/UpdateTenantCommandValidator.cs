using FluentValidation;

namespace Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Валидатор команды обновления тенанта
/// </summary>
internal sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantInt)
            .GreaterThan(0).WithMessage("TenantInt must be greater than zero.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name cannot be empty.")
            .MaximumLength(200).WithMessage("Tenant name must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

