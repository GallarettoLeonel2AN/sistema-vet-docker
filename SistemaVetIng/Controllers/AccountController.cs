using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewModels; 
using SistemaVetIng.ViewsModels; 
using System.Text.Encodings.Web; // Usado para codificar texto de forma segura en URLs.

namespace SistemaVetIng.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult RecuperarContraseña() => View();

        [HttpGet]
        public IActionResult ConfirmacionEnlaceReset() => View();

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? code = null, string? userId = null)
        {
            if (code == null || userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _accountService.EncontrarUsuarioId(userId);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new CambiarContraseñaViewModel { Code = code, Email = user.Email };
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(RecuperarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var code = await _accountService.GenerarPasswordResetToken(model.Email);
            if (code == null)
            {
                // Importante por seguridad: redirige sin dar pistas.
                return View("Login");
            }

            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = (await _accountService.EncontrarUsuarioPorEmail(model.Email))?.Id, code = code }, protocol: HttpContext.Request.Scheme);
            await _accountService.EnviarPasswordResetEmail(model.Email, HtmlEncoder.Default.Encode(callbackUrl));

            return View("ConfirmacionEnlaceReset");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(CambiarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors) = await _accountService.ResetPassword(model);
            if (success)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.PasswordSignIn(model);

            if (result.Succeeded)
            {
                var controllerName = await _accountService.GetRedireccionPorRol(model.UserName);

                if (controllerName != null)
                {
                    return RedirectToAction("PaginaPrincipal", controllerName);
                }

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "La cuenta está bloqueada.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}