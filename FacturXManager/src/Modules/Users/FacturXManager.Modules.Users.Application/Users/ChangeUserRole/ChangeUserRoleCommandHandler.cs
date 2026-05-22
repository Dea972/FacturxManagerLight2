using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Users.Application.Abstractions.Data;
using FacturXManager.Modules.Users.Domain.Users;

namespace FacturXManager.Modules.Users.Application.Users.ChangeUserRole;

internal sealed class ChangeUserRoleCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeUserRoleCommand>
{
    public async Task<Result> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        var newRole = Role.FromName(request.RoleName);

        if (newRole is null)
        {
            return Result.Failure(UserErrors.InvalidRole(request.RoleName));
        }

        user.ChangeRole(newRole);

        userRepository.UntrackStaticEntities();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
