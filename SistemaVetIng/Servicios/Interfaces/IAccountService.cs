using Microsoft.AspNetCore.Identity;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.ViewModels;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IAccountService
    {
        Task<SignInResult> PasswordSignIn(LoginViewModel model);
        Task<string?> GetRedireccionPorRol(string userName);
        Task<string?> GenerarPasswordResetToken(string email);
        Task<bool> EnviarPasswordResetEmail(string email, string callbackUrl);
        Task<(bool success, IEnumerable<IdentityError> errors)> ResetPassword(CambiarContraseñaViewModel model);
        Task<Usuario?> EncontrarUsuarioPorEmail(string email);
        Task<Usuario?> EncontrarUsuarioId(string userId);
        Task SignOut();
    }
}
