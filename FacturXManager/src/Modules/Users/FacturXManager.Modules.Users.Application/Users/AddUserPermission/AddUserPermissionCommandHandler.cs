using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Users.Application.Abstractions.Data;
using FacturXManager.Modules.Users.Domain.Users;

namespace FacturXManager.Modules.Users.Application.Users.AddUserPermission;

internal sealed class AddUserPermissionCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddUserPermissionCommand>
{
    public async Task<Result> Handle(AddUserPermissionCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        var permission = Permission.FromCode(request.PermissionCode);

        if (permission is null)
        {
            return Result.Failure(UserErrors.InvalidPermission(request.PermissionCode));
        }

        // Utilise AddUserPermission qui gère automatiquement le passage en rôle Custom
        user.AddUserPermission(permission);

        userRepository.UntrackStaticEntities();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
