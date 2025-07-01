// Controllers/AccountController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Models; // Asegúrate de que ApplicationUser esté aquí
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.ViewModels;
using SistemaVetIng.ViewsModels;
using System.Text.Encodings.Web; // Asegúrate de que LoginViewModel esté aquí

namespace SistemaVetIng.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager; // Puede que lo necesites para roles o si decides buscar al usuario
        private readonly IEmailSender _emailSender;

        public AccountController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager , IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult RecuperarContraseña()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                // Manejar error o redirigir
                return RedirectToAction(nameof(Login));
            }

            var model = new CambiarContraseñaViewModel { Code = code, Email = email };
            return View(model);
        }
        [HttpGet]
        public IActionResult Login(string? returnUrl = null) // returnUrl para redirección después del login
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken] // Siempre usa esto para formularios POST
        public async Task<IActionResult> ForgotPassword(RecuperarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // **IMPORTANTE**: Para evitar ataques de enumeración de usuarios,
            // siempre devuelve el mismo mensaje de confirmación,
            // sin importar si el correo existe o no.
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // No mostrar si el usuario no existe o no ha confirmado su correo.
                return View("ForgotPasswordConfirmation");
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

            return View("ForgotPasswordConfirmation");
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Siempre usa esto para formularios POST
        public async Task<IActionResult> ResetPassword(CambiarContraseñaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // No mostrar error si el usuario no existe.
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            // Restablecer la contraseña con el token
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

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Siempre usa esto para formularios POST
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Intentar iniciar sesión con el nombre de usuario y contraseña
                var result = await _signInManager.PasswordSignInAsync(
                    model.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false // Puedes habilitar bloqueo de cuenta por fallos
                );

                if (result.Succeeded)
                {
                    // ¡Inicio de sesión exitoso! Ahora verificamos el rol.
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Veterinario"))
                        {
                            // Redirigir a la vista de veterinario
                            return RedirectToAction("PaginaPrincipal", "Veterinario"); // Ejemplo de redirección
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Cliente"))
                        {
                            // Redirigir a la vista de cliente
                            return RedirectToAction("Index", "Home"); // O a tu vista de inicio del cliente
                        }
                        else
                        {
                            // Si tiene otros roles o ninguno de los esperados, redirigir a un Home por defecto
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    // Si returnUrl no es nulo y es local, redirigir allí.
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        // Si no hay returnUrl o no es local, redirigir al Home por defecto
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

            // Si ModelState no es válido, vuelve a mostrar el formulario con errores
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken] // Siempre usa esto para formularios POST
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Redirige al inicio después de cerrar sesión
        }
    }
}