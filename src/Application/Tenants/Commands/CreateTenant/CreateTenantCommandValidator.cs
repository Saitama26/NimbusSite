using FluentValidation;

namespace Application.Tenants.Commands.CreateTenant;

internal sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ConnectionString).NotEmpty().MaximumLength(500);
    }
}

