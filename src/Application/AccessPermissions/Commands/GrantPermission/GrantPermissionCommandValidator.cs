using FluentValidation;

namespace Application.AccessPermissions.Commands.GrantPermission;

internal sealed class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
{
    public GrantPermissionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
    }
}

