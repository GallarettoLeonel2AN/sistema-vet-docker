using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Models.Indentity; 
using SistemaVetIng.ViewModels; 
using SistemaVetIng.ViewsModels; 
using System.Text.Encodings.Web; // Usado para codificar texto de forma segura en URLs.

namespace SistemaVetIng.Controllers
{
    // Controladora cuentas => inicio de sesión, cierre de sesión y la recuperación de contraseñas.
    public class AccountController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager; // Gestiona el proceso de inicio y cierre de sesión.
        private readonly UserManager<Usuario> _userManager; //  Nos permite interactuar con los datos del usuario (buscar, asignar roles, generar tokens)
        private readonly IEmailSender _emailSender; // Servicio para enviar correos electrónicos => para la recuperación de contraseña.

        public AccountController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager, IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // --- Acciones HTTP GET (Para mostrar los formularios) ---
        [HttpGet]
        public IActionResult RecuperarContraseña()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ConfirmacionEnlaceReset()
        {
            return View();
        }

        [HttpGet]
       
        public async Task<IActionResult> ResetPassword(string code = null, string userId = null)
        {
            if (code == null || userId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Busca al usuario con el userId y obtén su email
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Maneja el caso en que el usuario no se encuentre
                return RedirectToAction(nameof(Login));
            }

            var model = new CambiarContraseñaViewModel { Code = code, Email = user.Email };
            return View(model);
        }

        // Mostrar el formulario de inicio de sesión.
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // --- Acciones HTTP POST (Para procesar datos de formularios) ---

        [HttpPost]
        [ValidateAntiForgeryToken] // Fundamental para prevenir ataques
        public async Task<IActionResult> ForgotPassword(RecuperarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Importante para la seguridad: evita enumeración de usuarios.
            if (user == null)
            {
                return View("Login");
            }
            // Generar el token de restablecimiento de contraseña
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Crear el enlace de callback para el correo electrónico
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

            // Enviar el correo electrónico con el enlace
            await _emailSender.SendEmailAsync(
                model.Email,
                "Restablecer Contraseña",
                $"Por favor, restablezca su contraseña haciendo clic en este enlace: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>enlace</a>"
            );

            return View("ConfirmacionEnlaceReset");
        }

        // ResetPassword Procesa el formulario donde el usuario ingresa su *nueva* contraseña.
        // Si el usuario no existe (o el token es inválido), redirige a la confirmación
        // Si el token es válido, se puede cambiar la contraseña del usuario.
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> ResetPassword(CambiarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // Mostrar la confirmación después de un restablecimiento de contraseña exitoso.
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // Inicio de sesión del usuario. Valida las credenciales proporcionadas.
        // Si el login es exitoso
        // Busca el usuario y, segun su rol ('Veterinario' 'Cliente' 'Administrador')
        // lo redirige a la página principal específica para ese rol.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Veterinario"))
                        {
                            return RedirectToAction("PaginaPrincipal", "Veterinario");
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Cliente"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return RedirectToAction("PaginaPrincipal", "Veterinaria");
                        }
                    }
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "La cuenta está bloqueada.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido.");
                    return View(model);
                }
            }
            return View(model);
        }

        // Cierre de sesión del usuario.
        // Redirige al usuario a la página de inicio.
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}