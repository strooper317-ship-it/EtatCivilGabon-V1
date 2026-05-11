using EtatCivilGabon.Models;
using System.ComponentModel.DataAnnotations;

namespace EtatCivilGabon.ViewModels
{
    // ── Tableau de bord agent ────────────────────────────────────────────────
    public class TableauDeBordAgentViewModel
    {
        public List<Demande> Demandes       { get; set; } = new();
        public int TotalEnAttente           { get; set; }
        public int TotalEnCours             { get; set; }
        public int TotalPret                { get; set; }
        public int TotalRejete              { get; set; }
        public string? FiltreStatut         { get; set; }
        public string? FiltreTypeActe       { get; set; }
        public string? FiltreRecherche      { get; set; }
    }

    // ── Traitement d'un dossier ──────────────────────────────────────────────
    public class TraiterDemandeViewModel
    {
        public int    Id               { get; set; }
        public string NumeroSuivi      { get; set; } = string.Empty;
        public string TypeActeLibelle  { get; set; } = string.Empty;
        public string NomDemandeur     { get; set; } = string.Empty;
        public string PrenomDemandeur  { get; set; } = string.Empty;
        public string ObjetDemande     { get; set; } = string.Empty;
        public StatutDemande StatutActuel  { get; set; }
        public DateTime  DateSoumission   { get; set; }
        public DateTime? DateMiseAJour    { get; set; }
        public string?   CommentaireActuel{ get; set; }
        public List<PieceJointe> PiecesJointes { get; set; } = new();

        // Document final déjà envoyé
        public string? NomDocumentFinalExistant   { get; set; }
        public DateTime? DateEnvoiDocumentExistant { get; set; }

        [Required(ErrorMessage = "Veuillez choisir un statut.")]
        [Display(Name = "Nouveau statut")]
        public StatutDemande NouveauStatut { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Commentaire")]
        public string? Commentaire { get; set; }

        // Nouveau document final à uploader (optionnel)
        [Display(Name = "Document final à envoyer au citoyen")]
        public IFormFile? DocumentFinal { get; set; }
    }
}
