using EtatCivilGabon.Data;
using EtatCivilGabon.Models;
using Microsoft.EntityFrameworkCore;

namespace EtatCivilGabon.Services
{
    public class DemandeService : IDemandeService
    {
        private readonly AppDbContext _context;
        private readonly NumeroSuiviGenerator _numeroGenerator;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DemandeService> _logger;

        public DemandeService(
            AppDbContext context,
            NumeroSuiviGenerator numeroGenerator,
            IWebHostEnvironment env,
            ILogger<DemandeService> logger)
        {
            _context         = context;
            _numeroGenerator = numeroGenerator;
            _env             = env;
            _logger          = logger;
        }

        // ── CRÉER UNE DEMANDE ────────────────────────────────────────────────
        public async Task<Demande> CreerDemandeAsync(
            Demande demande, List<IFormFile> fichiers, string citoyenId)
        {
            demande.CitoyenId      = citoyenId;
            demande.NumeroSuivi    = await _numeroGenerator.GenererAsync();
            demande.Statut         = StatutDemande.EnAttente;
            demande.DateSoumission = DateTime.Now;

            _context.Demandes.Add(demande);
            await _context.SaveChangesAsync();

            if (fichiers.Count > 0)
            {
                var dossier = Path.Combine(
                    _env.WebRootPath, "uploads", demande.NumeroSuivi);
                Directory.CreateDirectory(dossier);

                foreach (var fichier in fichiers)
                {
                    if (fichier.Length == 0) continue;

                    var nomFichier    = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(fichier.FileName)}";
                    var cheminFichier = Path.Combine(dossier, nomFichier);

                    using var stream = new FileStream(cheminFichier, FileMode.Create);
                    await fichier.CopyToAsync(stream);

                    _context.PiecesJointes.Add(new PieceJointe
                    {
                        DemandeId     = demande.Id,
                        NomFichier    = fichier.FileName,
                        CheminFichier = cheminFichier,
                        TypeMime      = fichier.ContentType,
                        Taille        = fichier.Length,
                        DateDepot     = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Nouvelle demande créée : {NumeroSuivi} pour {CitoyenId}",
                demande.NumeroSuivi, citoyenId);

            return demande;
        }

        // ── PAR NUMÉRO DE SUIVI ──────────────────────────────────────────────
        public async Task<Demande?> ObtenirParNumeroSuiviAsync(string numeroSuivi)
        {
            return await _context.Demandes
                .Include(d => d.TypeActe)
                .Include(d => d.PiecesJointes)
                .FirstOrDefaultAsync(d => d.NumeroSuivi == numeroSuivi);
        }

        // ── DEMANDES D'UN CITOYEN ────────────────────────────────────────────
        public async Task<IEnumerable<Demande>> ObtenirDemandesCitoyenAsync(string citoyenId)
        {
            return await _context.Demandes
                .Include(d => d.TypeActe)
                .Include(d => d.PiecesJointes)
                .Where(d => d.CitoyenId == citoyenId)
                .OrderByDescending(d => d.DateSoumission)
                .ToListAsync();
        }

        // ── TOUTES LES DEMANDES ──────────────────────────────────────────────
        public async Task<IEnumerable<Demande>> ObtenirToutesLesDemandesAsync()
        {
            return await _context.Demandes
                .Include(d => d.TypeActe)
                .Include(d => d.PiecesJointes)
                .OrderByDescending(d => d.DateSoumission)
                .ToListAsync();
        }

        // ── PAR ID ───────────────────────────────────────────────────────────
        public async Task<Demande?> ObtenirParIdAsync(int id)
        {
            return await _context.Demandes
                .Include(d => d.TypeActe)
                .Include(d => d.PiecesJointes)
                .Include(d => d.Notifications)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        // ── METTRE À JOUR LE STATUT (avec document final optionnel) ──────────
        public async Task MettreAJourStatutAsync(
            int id,
            StatutDemande statut,
            string commentaire,
            string? cheminDocumentFinal = null,
            string? nomDocumentFinal    = null,
            string? typeMimeDocument    = null,
            long?   tailleDocument      = null)
        {
            var demande = await _context.Demandes.FindAsync(id);
            if (demande == null) return;

            demande.Statut           = statut;
            demande.CommentaireAgent = commentaire;
            demande.DateMiseAJour    = DateTime.Now;

            // Enregistrer le document final si fourni
            if (!string.IsNullOrEmpty(cheminDocumentFinal))
            {
                demande.CheminDocumentFinal   = cheminDocumentFinal;
                demande.NomDocumentFinal      = nomDocumentFinal;
                demande.TypeMimeDocumentFinal = typeMimeDocument;
                demande.TailleDocumentFinal   = tailleDocument;
                demande.DateEnvoiDocument     = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Statut demande {NumeroSuivi} → {Statut}",
                demande.NumeroSuivi, statut);
        }

        // ── STATISTIQUES ─────────────────────────────────────────────────────
        public async Task<Dictionary<string, int>> ObtenirStatistiquesAsync()
        {
            return new Dictionary<string, int>
            {
                ["TotalDemandes"] = await _context.Demandes.CountAsync(),
                ["EnAttente"]     = await _context.Demandes.CountAsync(d => d.Statut == StatutDemande.EnAttente),
                ["EnCours"]       = await _context.Demandes.CountAsync(d => d.Statut == StatutDemande.EnCours),
                ["Pret"]          = await _context.Demandes.CountAsync(d => d.Statut == StatutDemande.Pret),
                ["Rejete"]        = await _context.Demandes.CountAsync(d => d.Statut == StatutDemande.Rejete),
                ["ActeNaissance"] = await _context.Demandes.CountAsync(d => d.TypeActeId == 1),
                ["ActeMariage"]   = await _context.Demandes.CountAsync(d => d.TypeActeId == 2),
                ["ActeDeces"]     = await _context.Demandes.CountAsync(d => d.TypeActeId == 3),
            };
        }
    }
}
