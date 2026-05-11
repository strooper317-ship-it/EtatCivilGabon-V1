using EtatCivilGabon.Data;
using EtatCivilGabon.Models;
using EtatCivilGabon.Services;
using EtatCivilGabon.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtatCivilGabon.Controllers
{
    [Authorize(Roles = "Citoyen")]
    public class CitoyenController : Controller
    {
        private readonly IDemandeService _demandeService;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public CitoyenController(
            IDemandeService demandeService,
            UserManager<Utilisateur> userManager,
            AppDbContext context,
            INotificationService notificationService)
        {
            _demandeService      = demandeService;
            _userManager         = userManager;
            _context             = context;
            _notificationService = notificationService;
        }

        // ── TABLEAU DE BORD ──────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var citoyen = await _userManager.GetUserAsync(User);
            if (citoyen == null) return RedirectToAction("Connexion", "Compte");

            var demandes = (await _demandeService.ObtenirDemandesCitoyenAsync(citoyen.Id)).ToList();

            var vm = new MesDemandesViewModel
            {
                Demandes       = demandes,
                TotalEnAttente = demandes.Count(d => d.Statut == StatutDemande.EnAttente),
                TotalEnCours   = demandes.Count(d => d.Statut == StatutDemande.EnCours),
                TotalPret      = demandes.Count(d => d.Statut == StatutDemande.Pret),
                TotalRejete    = demandes.Count(d => d.Statut == StatutDemande.Rejete),
            };

            ViewBag.NomComplet = citoyen.NomComplet;
            return View(vm);
        }

        // ── NOUVELLE DEMANDE ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> NouvelleDemande()
        {
            var vm = new NouvelleDemandViewModel
            {
                TypesActes = await _context.TypesActes.Where(t => t.EstActif).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NouvelleDemande(NouvelleDemandViewModel vm)
        {
            vm.TypesActes = await _context.TypesActes.Where(t => t.EstActif).ToListAsync();

            if (!ModelState.IsValid)
                return View(vm);

            var extsAutorisees = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            foreach (var f in vm.Fichiers)
            {
                var ext = Path.GetExtension(f.FileName).ToLower();
                if (!extsAutorisees.Contains(ext))
                {
                    ModelState.AddModelError("Fichiers",
                        $"'{f.FileName}' n'est pas autorisé. Formats acceptés : PDF, JPG, PNG.");
                    return View(vm);
                }
                if (f.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("Fichiers",
                        $"'{f.FileName}' dépasse la limite de 5 Mo.");
                    return View(vm);
                }
            }

            var citoyen = await _userManager.GetUserAsync(User);
            if (citoyen == null) return RedirectToAction("Connexion", "Compte");

            var demande = new Demande
            {
                TypeActeId      = vm.TypeActeId,
                NomDemandeur    = vm.NomDemandeur,
                PrenomDemandeur = vm.PrenomDemandeur,
                ObjetDemande    = vm.ObjetDemande,
            };

            var creee = await _demandeService.CreerDemandeAsync(demande, vm.Fichiers, citoyen.Id);

            if (!string.IsNullOrEmpty(citoyen.Email))
            {
                var demandeAvecType = await _demandeService.ObtenirParIdAsync(creee.Id);
                if (demandeAvecType != null)
                    await _notificationService.EnvoyerConfirmationDemandeAsync(
                        demandeAvecType, citoyen.Email);
            }

            TempData["Succes"] = $"Demande soumise avec succès ! Numéro de suivi : {creee.NumeroSuivi}";
            return RedirectToAction(nameof(Detail), new { id = creee.Id });
        }

        // ── DÉTAIL ───────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var citoyen = await _userManager.GetUserAsync(User);
            if (citoyen == null) return RedirectToAction("Connexion", "Compte");

            var demande = await _demandeService.ObtenirParIdAsync(id);
            if (demande == null) return NotFound();
            if (demande.CitoyenId != citoyen.Id) return Forbid();

            var vm = MapperDetailVm(demande);
            return View(vm);
        }

        // ── SUIVI PUBLIC ─────────────────────────────────────────────────────
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Suivi() => View(new SuiviPublicViewModel());

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suivi(SuiviPublicViewModel vm)
        {
            vm.RechercheEffectuee = true;
            if (!ModelState.IsValid) return View(vm);

            var demande = await _demandeService
                .ObtenirParNumeroSuiviAsync(vm.NumeroSuivi.Trim().ToUpper());

            if (demande != null)
                vm.Resultat = MapperDetailVm(demande);

            return View(vm);
        }

        // ── HELPER ──────────────────────────────────────────────────────────
        private static DetailDemandeViewModel MapperDetailVm(Demande d) => new()
        {
            Id                = d.Id,
            NumeroSuivi       = d.NumeroSuivi,
            TypeActeLibelle   = d.TypeActe?.Libelle ?? "-",
            NomDemandeur      = d.NomDemandeur,
            PrenomDemandeur   = d.PrenomDemandeur,
            ObjetDemande      = d.ObjetDemande,
            Statut            = d.Statut,
            DateSoumission    = d.DateSoumission,
            DateMiseAJour     = d.DateMiseAJour,
            CommentaireAgent  = d.CommentaireAgent,
            PiecesJointes     = d.PiecesJointes.ToList(),
            DelaiCible        = d.TypeActe?.DelaiCible ?? 0,
            NomDocumentFinal  = d.NomDocumentFinal,
            DateEnvoiDocument = d.DateEnvoiDocument,
        };
    }
}
