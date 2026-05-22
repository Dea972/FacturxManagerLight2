using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Users.Application.Abstractions.Data;
using FacturXManager.Modules.Users.Domain.Users;

namespace FacturXManager.Modules.Users.Application.Users.RemoveUserPermission;

internal sealed class RemoveUserPermissionCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveUserPermissionCommand>
{
    public async Task<Result> Handle(RemoveUserPermissionCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        // Utilise RemoveUserPermission qui ne fonctionne que si rôle Custom
        user.RemoveUserPermission(request.PermissionCode);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
