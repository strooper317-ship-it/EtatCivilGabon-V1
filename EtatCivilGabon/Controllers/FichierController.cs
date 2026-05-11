using EtatCivilGabon.Data;
using EtatCivilGabon.Models;
using EtatCivilGabon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtatCivilGabon.Controllers
{
    [Authorize]
    public class FichierController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDemandeService _demandeService;
        private readonly UserManager<Utilisateur> _userManager;

        public FichierController(
            AppDbContext context,
            IDemandeService demandeService,
            UserManager<Utilisateur> userManager)
        {
            _context        = context;
            _demandeService = demandeService;
            _userManager    = userManager;
        }

        // ── Visualiser une pièce jointe dans le navigateur ───────────────────
        [HttpGet]
        public async Task<IActionResult> Voir(int id)
        {
            var (piece, erreur) = await VerifierAccesPieceAsync(id);
            if (erreur != null) return erreur;

            if (!System.IO.File.Exists(piece!.CheminFichier))
                return NotFound("Fichier introuvable sur le serveur.");

            var mime   = ObtenirMime(piece.NomFichier, piece.TypeMime);
            var octets = await System.IO.File.ReadAllBytesAsync(piece.CheminFichier);

            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{Uri.EscapeDataString(piece.NomFichier)}\"";

            return File(octets, mime);
        }

        // ── Télécharger une pièce jointe ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Telecharger(int id)
        {
            var (piece, erreur) = await VerifierAccesPieceAsync(id);
            if (erreur != null) return erreur;

            if (!System.IO.File.Exists(piece!.CheminFichier))
                return NotFound("Fichier introuvable sur le serveur.");

            var mime   = ObtenirMime(piece.NomFichier, piece.TypeMime);
            var octets = await System.IO.File.ReadAllBytesAsync(piece.CheminFichier);

            return File(octets, mime, piece.NomFichier);
        }

        // ── Visualiser le document final envoyé par l'agent ──────────────────
        [HttpGet]
        public async Task<IActionResult> VoirDocumentFinal(int demandeId)
        {
            var (demande, erreur) = await VerifierAccesDocumentFinalAsync(demandeId);
            if (erreur != null) return erreur;

            if (!System.IO.File.Exists(demande!.CheminDocumentFinal))
                return NotFound("Document introuvable sur le serveur.");

            var mime   = ObtenirMime(demande.NomDocumentFinal!, demande.TypeMimeDocumentFinal);
            var octets = await System.IO.File.ReadAllBytesAsync(demande.CheminDocumentFinal!);

            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{Uri.EscapeDataString(demande.NomDocumentFinal!)}\"";

            return File(octets, mime);
        }

        // ── Télécharger le document final ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> TelechargerDocumentFinal(int demandeId)
        {
            var (demande, erreur) = await VerifierAccesDocumentFinalAsync(demandeId);
            if (erreur != null) return erreur;

            if (!System.IO.File.Exists(demande!.CheminDocumentFinal))
                return NotFound("Document introuvable sur le serveur.");

            var mime   = ObtenirMime(demande.NomDocumentFinal!, demande.TypeMimeDocumentFinal);
            var octets = await System.IO.File.ReadAllBytesAsync(demande.CheminDocumentFinal!);

            return File(octets, mime, demande.NomDocumentFinal!);
        }

        // ── Vérification accès pièce jointe ──────────────────────────────────
        private async Task<(PieceJointe? piece, IActionResult? erreur)>
            VerifierAccesPieceAsync(int pieceId)
        {
            var piece = await _context.PiecesJointes
                .Include(p => p.Demande)
                .FirstOrDefaultAsync(p => p.Id == pieceId);

            if (piece == null)
                return (null, NotFound());

            var demande = piece.Demande;
            if (demande == null)
                return (null, NotFound());

            var utilisateur = await _userManager.GetUserAsync(User);
            if (utilisateur == null)
                return (null, Forbid());

            // Citoyen : uniquement ses propres documents
            if (User.IsInRole("Citoyen") && demande.CitoyenId != utilisateur.Id)
                return (null, Forbid());

            return (piece, null);
        }

        // ── Vérification accès document final ────────────────────────────────
        private async Task<(Demande? demande, IActionResult? erreur)>
            VerifierAccesDocumentFinalAsync(int demandeId)
        {
            var demande = await _context.Demandes
                .FirstOrDefaultAsync(d => d.Id == demandeId);

            if (demande == null)
                return (null, NotFound());

            // Document final non disponible
            if (string.IsNullOrEmpty(demande.CheminDocumentFinal))
                return (null, NotFound("Aucun document final disponible pour cette demande."));

            var utilisateur = await _userManager.GetUserAsync(User);
            if (utilisateur == null)
                return (null, Forbid());

            // Citoyen : uniquement sa propre demande
            if (User.IsInRole("Citoyen") && demande.CitoyenId != utilisateur.Id)
                return (null, Forbid());

            return (demande, null);
        }

        // ── Déterminer le type MIME ──────────────────────────────────────────
        private static string ObtenirMime(string nomFichier, string? mimeStocke)
        {
            if (!string.IsNullOrEmpty(mimeStocke)) return mimeStocke;

            return Path.GetExtension(nomFichier).ToLower() switch
            {
                ".pdf"  => "application/pdf",
                ".jpg"  => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                ".gif"  => "image/gif",
                _       => "application/octet-stream"
            };
        }
    }
}
