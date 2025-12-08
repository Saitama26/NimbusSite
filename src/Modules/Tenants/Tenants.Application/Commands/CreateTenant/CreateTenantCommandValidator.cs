using FluentValidation;

namespace Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Валидатор команды создания тенанта
/// </summary>
internal sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(200).WithMessage("Tenant name must not exceed 200 characters.");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required.")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters long.")
            .MaximumLength(63).WithMessage("Subdomain must not exceed 63 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")
            .WithMessage("Subdomain must contain only lowercase letters, numbers and hyphens, and cannot start or end with a hyphen.");

        RuleFor(x => x.AdminEmail)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrWhiteSpace(x.AdminEmail));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

