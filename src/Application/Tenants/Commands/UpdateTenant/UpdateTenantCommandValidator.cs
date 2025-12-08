using FluentValidation;

namespace Application.Tenants.Commands.UpdateTenant;

internal sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => x.Name is not null);
        RuleFor(x => x.ConnectionString)
            .MaximumLength(500)
            .When(x => x.ConnectionString is not null);
    }
}

