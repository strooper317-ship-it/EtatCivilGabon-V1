using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EtatCivilGabon.Models;

namespace EtatCivilGabon.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<Utilisateur> _userManager;

        public HomeController(UserManager<Utilisateur> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Redirige selon le rôle si déjà connecté
            if (User.Identity?.IsAuthenticated == true)
            {
                var utilisateur = await _userManager.GetUserAsync(User);
                if (utilisateur != null)
                {
                    if (await _userManager.IsInRoleAsync(utilisateur, "Administrateur"))
                        return RedirectToAction("Index", "Admin");

                    if (await _userManager.IsInRoleAsync(utilisateur, "Agent"))
                        return RedirectToAction("Index", "Agent");

                    return RedirectToAction("Index", "Citoyen");
                }
            }

            return View();
        }
    }
}
