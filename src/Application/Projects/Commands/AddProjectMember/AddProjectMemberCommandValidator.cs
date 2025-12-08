using FluentValidation;

namespace Application.Projects.Commands.AddProjectMember;

internal sealed class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

