using EtatCivilGabon.Data;
using EtatCivilGabon.Models;
using EtatCivilGabon.Services;
using EtatCivilGabon.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EtatCivilGabon.Controllers
{
    [Authorize(Roles = "Administrateur")]
    public class AdminController : Controller
    {
        private readonly IDemandeService _demandeService;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly AppDbContext _context;

        public AdminController(
            IDemandeService demandeService,
            UserManager<Utilisateur> userManager,
            AppDbContext context)
        {
            _demandeService = demandeService;
            _userManager    = userManager;
            _context        = context;
        }

        // ── TABLEAU DE BORD ──────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var stats    = await _demandeService.ObtenirStatistiquesAsync();
            var toutes   = (await _demandeService.ObtenirToutesLesDemandesAsync()).ToList();
            var agents   = await _userManager.GetUsersInRoleAsync("Agent");

            // Délai moyen de traitement (demandes prêtes ou rejetées)
            var terminees = toutes
                .Where(d => d.DateMiseAJour.HasValue &&
                           (d.Statut == StatutDemande.Pret || d.Statut == StatutDemande.Rejete))
                .ToList();

            double delaiMoyen = terminees.Any()
                ? terminees.Average(d =>
                    (d.DateMiseAJour!.Value - d.DateSoumission).TotalDays)
                : 0;

            double tauxRejet = stats["TotalDemandes"] > 0
                ? Math.Round((double)stats["Rejete"] / stats["TotalDemandes"] * 100, 1)
                : 0;

            // Statistiques mensuelles sur les 6 derniers mois
            var statsMensuelles = new List<StatMensuelle>();
            for (int i = 5; i >= 0; i--)
            {
                var mois = DateTime.Now.AddMonths(-i);
                var count = toutes.Count(d =>
                    d.DateSoumission.Year  == mois.Year &&
                    d.DateSoumission.Month == mois.Month);

                statsMensuelles.Add(new StatMensuelle
                {
                    Mois   = mois.ToString("MMM yyyy"),
                    Nombre = count
                });
            }

            var vm = new TableauDeBordAdminViewModel
            {
                TotalDemandes       = stats["TotalDemandes"],
                TotalEnAttente      = stats["EnAttente"],
                TotalEnCours        = stats["EnCours"],
                TotalPret           = stats["Pret"],
                TotalRejete         = stats["Rejete"],
                TotalActeNaissance  = stats["ActeNaissance"],
                TotalActeMariage    = stats["ActeMariage"],
                TotalActeDeces      = stats["ActeDeces"],
                DelaiMoyenTraitement= Math.Round(delaiMoyen, 1),
                TauxRejet           = tauxRejet,
                NombreAgents        = agents.Count,
                DernieresDemandes   = toutes.Take(10).ToList(),
                StatsMensuelles     = statsMensuelles,
            };

            return View(vm);
        }

        // ── GESTION DES AGENTS ───────────────────────────────────────────────
        public async Task<IActionResult> Agents()
        {
            var agents  = await _userManager.GetUsersInRoleAsync("Agent");
            var toutes  = (await _demandeService.ObtenirToutesLesDemandesAsync()).ToList();

            var vm = new GestionAgentsViewModel
            {
                Agents = agents.Select(a => new AgentViewModel
                {
                    Id              = a.Id,
                    NomComplet      = a.NomComplet,
                    Email           = a.Email ?? "-",
                    EstActif        = a.EstActif,
                    DateInscription = a.DateInscription,
                    NombreDossiers  = 0, // Phase future : tracker les traitements par agent
                }).OrderBy(a => a.NomComplet).ToList()
            };

            return View(vm);
        }

        // ── CRÉER UN AGENT ───────────────────────────────────────────────────
        [HttpGet]
        public IActionResult CreerAgent() => View(new CreerAgentViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreerAgent(CreerAgentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var agent = new Utilisateur
            {
                UserName  = vm.Email,
                Email     = vm.Email,
                Nom       = vm.Nom,
                Prenom    = vm.Prenom,
                EstActif  = true,
                EmailConfirmed = true,
            };

            var resultat = await _userManager.CreateAsync(agent, vm.MotDePasse);
            if (resultat.Succeeded)
            {
                await _userManager.AddToRoleAsync(agent, "Agent");
                TempData["Succes"] = $"Agent {agent.NomComplet} créé avec succès.";
                return RedirectToAction(nameof(Agents));
            }

            foreach (var e in resultat.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(vm);
        }

        // ── ACTIVER / DÉSACTIVER UN AGENT ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BasculerStatutAgent(string id)
        {
            var agent = await _userManager.FindByIdAsync(id);
            if (agent == null) return NotFound();

            agent.EstActif = !agent.EstActif;
            await _userManager.UpdateAsync(agent);

            TempData["Succes"] = agent.EstActif
                ? $"Le compte de {agent.NomComplet} a été réactivé."
                : $"Le compte de {agent.NomComplet} a été désactivé.";

            return RedirectToAction(nameof(Agents));
        }

        // ── TOUTES LES DEMANDES (vue admin) ──────────────────────────────────
        public async Task<IActionResult> Demandes(
            string? filtreStatut    = null,
            string? filtreTypeActe  = null,
            string? filtreRecherche = null)
        {
            var toutes = (await _demandeService.ObtenirToutesLesDemandesAsync()).ToList();
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

            ViewBag.FiltreStatut    = filtreStatut;
            ViewBag.FiltreTypeActe  = filtreTypeActe;
            ViewBag.FiltreRecherche = filtreRecherche;
            ViewBag.Total           = toutes.Count;

            return View(filtre.ToList());
        }

        // ── DÉTAIL D'UNE DEMANDE (visualisation pièces jointes) ─────────────
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var demande = await _demandeService.ObtenirParIdAsync(id);
            if (demande == null) return NotFound();
            return View(demande);
        }

        // ── EXPORT CSV ───────────────────────────────────────────────────────
        public async Task<IActionResult> ExporterCsv()
        {
            var demandes = await _demandeService.ObtenirToutesLesDemandesAsync();

            var sb = new StringBuilder();
            sb.AppendLine("NumeroSuivi;TypeActe;NomDemandeur;PrenomDemandeur;ObjetDemande;Statut;DateSoumission;DateMiseAJour;CommentaireAgent");

            foreach (var d in demandes)
            {
                sb.AppendLine(string.Join(";",
                    Echapper(d.NumeroSuivi),
                    Echapper(d.TypeActe?.Libelle ?? "-"),
                    Echapper(d.NomDemandeur),
                    Echapper(d.PrenomDemandeur),
                    Echapper(d.ObjetDemande),
                    Echapper(d.Statut.ToString()),
                    d.DateSoumission.ToString("dd/MM/yyyy HH:mm"),
                    d.DateMiseAJour?.ToString("dd/MM/yyyy HH:mm") ?? "-",
                    Echapper(d.CommentaireAgent ?? "")
                ));
            }

            var bytes    = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();
            var nomFichier = $"demandes_etatcivil_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            return File(bytes, "text/csv; charset=utf-8", nomFichier);
        }

        // ── HELPER ──────────────────────────────────────────────────────────
        private static string Echapper(string val)
            => $"\"{val.Replace("\"", "\"\"")}\"";
    }
}
