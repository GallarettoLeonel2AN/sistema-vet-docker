using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;

namespace SistemaVetIng.Controllers
{
    public class TurnoController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        public TurnoController(
            ApplicationDbContext context,
            IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

    }
}
