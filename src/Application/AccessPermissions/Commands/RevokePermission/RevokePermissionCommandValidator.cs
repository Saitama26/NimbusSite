using FluentValidation;

namespace Application.AccessPermissions.Commands.RevokePermission;

internal sealed class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
{
    public RevokePermissionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}

