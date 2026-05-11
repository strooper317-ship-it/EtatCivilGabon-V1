using EtatCivilGabon.Models;
using EtatCivilGabon.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtatCivilGabon.Controllers
{
    public class CompteController : Controller
    {
        private readonly UserManager<Utilisateur> _userManager;
        private readonly SignInManager<Utilisateur> _signInManager;

        public CompteController(
            UserManager<Utilisateur> userManager,
            SignInManager<Utilisateur> signInManager)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
        }

        // ── INSCRIPTION ──────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Inscription()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscription(InscriptionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var utilisateur = new Utilisateur
            {
                UserName = model.Email,
                Email    = model.Email,
                Nom      = model.Nom,
                Prenom   = model.Prenom,
                EstActif = true
            };

            var resultat = await _userManager.CreateAsync(utilisateur, model.MotDePasse);

            if (resultat.Succeeded)
            {
                await _userManager.AddToRoleAsync(utilisateur, "Citoyen");
                await _signInManager.SignInAsync(utilisateur, isPersistent: false);
                TempData["Succes"] = "Votre compte a été créé avec succès. Bienvenue !";
                return RedirectToAction("Index", "Citoyen");
            }

            foreach (var erreur in resultat.Errors)
                ModelState.AddModelError(string.Empty, erreur.Description);

            return View(model);
        }

        // ── CONNEXION ────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Connexion(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Connexion(ConnexionViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var resultat = await _signInManager.PasswordSignInAsync(
                model.Email, model.MotDePasse, model.Sesouvenir, lockoutOnFailure: true);

            if (resultat.Succeeded)
            {
                var utilisateur = await _userManager.FindByEmailAsync(model.Email);

                if (utilisateur == null || !utilisateur.EstActif)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Compte désactivé. Contactez l'administrateur.");
                    return View(model);
                }

                if (await _userManager.IsInRoleAsync(utilisateur, "Administrateur"))
                    return RedirectToLocal(returnUrl, "Index", "Admin");

                if (await _userManager.IsInRoleAsync(utilisateur, "Agent"))
                    return RedirectToLocal(returnUrl, "Index", "Agent");

                return RedirectToLocal(returnUrl, "Index", "Citoyen");
            }

            if (resultat.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Compte bloqué après trop de tentatives. Réessayez dans 15 minutes.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Adresse e-mail ou mot de passe incorrect.");
            return View(model);
        }

        // ── DÉCONNEXION ──────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Deconnexion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Connexion");
        }

        // ── ACCÈS REFUSÉ ─────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult AccesRefuse() => View();

        // ── CHANGER MOT DE PASSE ─────────────────────────────────────────────

        [HttpGet]
        [Authorize]
        public IActionResult ChangerMotDePasse() => View();

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangerMotDePasse(ChangerMotDePasseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var utilisateur = await _userManager.GetUserAsync(User);
            if (utilisateur == null) return RedirectToAction("Connexion");

            var resultat = await _userManager.ChangePasswordAsync(
                utilisateur, model.AncienMotDePasse, model.NouveauMotDePasse);

            if (resultat.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(utilisateur);
                TempData["Succes"] = "Mot de passe modifié avec succès.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var erreur in resultat.Errors)
                ModelState.AddModelError(string.Empty, erreur.Description);

            return View(model);
        }

        // ── HELPER ──────────────────────────────────────────────────────────

        private IActionResult RedirectToLocal(string? returnUrl, string action, string controller)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(action, controller);
        }
    }
}
