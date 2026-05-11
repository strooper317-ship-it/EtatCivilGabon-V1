using EtatCivilGabon.Models;
using EtatCivilGabon.Services;
using EtatCivilGabon.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtatCivilGabon.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        private readonly IDemandeService _demandeService;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IWebHostEnvironment _env;

        public AgentController(
            IDemandeService demandeService,
            UserManager<Utilisateur> userManager,
            INotificationService notificationService,
            IWebHostEnvironment env)
        {
            _demandeService      = demandeService;
            _userManager         = userManager;
            _notificationService = notificationService;
            _env                 = env;
        }

        // ── TABLEAU DE BORD ──────────────────────────────────────────────────
        public async Task<IActionResult> Index(
            string? filtreStatut    = null,
            string? filtreTypeActe  = null,
            string? filtreRecherche = null)
        {
            var toutes = (await _demandeService.ObtenirToutesLesDemandesAsync()).ToList();

            var vm = new TableauDeBordAgentViewModel
            {
                TotalEnAttente  = toutes.Count(d => d.Statut == StatutDemande.EnAttente),
                TotalEnCours    = toutes.Count(d => d.Statut == StatutDemande.EnCours),
                TotalPret       = toutes.Count(d => d.Statut == StatutDemande.Pret),
                TotalRejete     = toutes.Count(d => d.Statut == StatutDemande.Rejete),
                FiltreStatut    = filtreStatut,
                FiltreTypeActe  = filtreTypeActe,
                FiltreRecherche = filtreRecherche,
            };

            var filtre = toutes.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtreStatut)
                && Enum.TryParse<StatutDemande>(filtreStatut, out var statut))
                filtre = filtre.Where(d => d.Statut == statut);

            if (!string.IsNullOrWhiteSpace(filtreTypeActe)
                && int.TryParse(filtreTypeActe, out var typeId))
                filtre = filtre.Where(d => d.TypeActeId == typeId);

            if (!string.IsNullOrWhiteSpace(filtreRecherche))
            {
                var q = filtreRecherche.ToLower();
                filtre = filtre.Where(d =>
                    d.NumeroSuivi.ToLower().Contains(q) ||
                    d.NomDemandeur.ToLower().Contains(q) ||
                    d.PrenomDemandeur.ToLower().Contains(q));
            }

            vm.Demandes = filtre.ToList();
            return View(vm);
        }

        // ── TRAITER UN DOSSIER ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Traiter(int id)
        {
            var demande = await _demandeService.ObtenirParIdAsync(id);
            if (demande == null) return NotFound();

            var vm = MapperVm(demande);
            ViewData["Demande"] = demande;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Traiter(TraiterDemandeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var d = await _demandeService.ObtenirParIdAsync(vm.Id);
                if (d != null)
                {
                    vm.PiecesJointes = d.PiecesJointes.ToList();
                    ViewData["Demande"] = d;
                }
                return View(vm);
            }

            // ── Validation du document final ──────────────────────────────────
            if (vm.DocumentFinal != null && vm.DocumentFinal.Length > 0)
            {
                var extsAutorisees = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(vm.DocumentFinal.FileName).ToLower();
                if (!extsAutorisees.Contains(ext))
                {
                    ModelState.AddModelError("DocumentFinal",
                        "Format non autorisé. Utilisez PDF, JPG ou PNG.");
                    var d2 = await _demandeService.ObtenirParIdAsync(vm.Id);
                    if (d2 != null) vm.PiecesJointes = d2.PiecesJointes.ToList();
                    return View(vm);
                }
                if (vm.DocumentFinal.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("DocumentFinal",
                        "Le document dépasse la taille maximale de 10 Mo.");
                    var d2 = await _demandeService.ObtenirParIdAsync(vm.Id);
                    if (d2 != null) vm.PiecesJointes = d2.PiecesJointes.ToList();
                    return View(vm);
                }
            }

            // ── Sauvegarder le document final ────────────────────────────────
            string? cheminDoc = null;
            string? nomDoc    = null;
            string? mimeDoc   = null;
            long?   tailleDoc = null;

            if (vm.DocumentFinal != null && vm.DocumentFinal.Length > 0)
            {
                var dossier = Path.Combine(
                    _env.WebRootPath, "documents-finaux");
                Directory.CreateDirectory(dossier);

                nomDoc    = vm.DocumentFinal.FileName;
                mimeDoc   = vm.DocumentFinal.ContentType;
                tailleDoc = vm.DocumentFinal.Length;

                var nomFichierServeur = $"{vm.Id}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(nomDoc)}";
                cheminDoc = Path.Combine(dossier, nomFichierServeur);

                using var stream = new FileStream(cheminDoc, FileMode.Create);
                await vm.DocumentFinal.CopyToAsync(stream);
            }

            // ── Mettre à jour le statut ───────────────────────────────────────
            await _demandeService.MettreAJourStatutAsync(
                vm.Id,
                vm.NouveauStatut,
                vm.Commentaire ?? string.Empty,
                cheminDoc, nomDoc, mimeDoc, tailleDoc);

            // ── Notifier le citoyen ───────────────────────────────────────────
            var demandeMAJ = await _demandeService.ObtenirParIdAsync(vm.Id);
            if (demandeMAJ != null)
            {
                var citoyen = await _userManager.FindByIdAsync(demandeMAJ.CitoyenId);
                if (citoyen?.Email != null)
                {
                    if (vm.NouveauStatut == StatutDemande.Pret)
                        await _notificationService.EnvoyerDocumentPretAsync(demandeMAJ, citoyen.Email);
                    else
                        await _notificationService.EnvoyerChangementStatutAsync(demandeMAJ, citoyen.Email);
                }
            }

            TempData["Succes"] =
                $"Dossier {vm.NumeroSuivi} mis à jour : statut → {LibelleStatut(vm.NouveauStatut)}.";

            return RedirectToAction(nameof(Index));
        }

        // ── CONSULTER (lecture seule) ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Consulter(int id)
        {
            var demande = await _demandeService.ObtenirParIdAsync(id);
            if (demande == null) return NotFound();

            var vm = MapperVm(demande);
            ViewData["Demande"]      = demande;
            ViewBag.LectureSeule     = true;
            return View("Traiter", vm);
        }

        // ── HELPERS ─────────────────────────────────────────────────────────
        private static TraiterDemandeViewModel MapperVm(Demande d) => new()
        {
            Id                          = d.Id,
            NumeroSuivi                 = d.NumeroSuivi,
            TypeActeLibelle             = d.TypeActe?.Libelle ?? "-",
            NomDemandeur                = d.NomDemandeur,
            PrenomDemandeur             = d.PrenomDemandeur,
            ObjetDemande                = d.ObjetDemande,
            StatutActuel                = d.Statut,
            DateSoumission              = d.DateSoumission,
            DateMiseAJour               = d.DateMiseAJour,
            CommentaireActuel           = d.CommentaireAgent,
            PiecesJointes               = d.PiecesJointes.ToList(),
            NouveauStatut               = d.Statut,
            Commentaire                 = d.CommentaireAgent,
            NomDocumentFinalExistant    = d.NomDocumentFinal,
            DateEnvoiDocumentExistant   = d.DateEnvoiDocument,
        };

        private static string LibelleStatut(StatutDemande s) => s switch
        {
            StatutDemande.EnAttente => "En attente",
            StatutDemande.EnCours   => "En cours",
            StatutDemande.Pret      => "Prêt",
            StatutDemande.Rejete    => "Rejeté",
            _                       => s.ToString()
        };
    }
}
