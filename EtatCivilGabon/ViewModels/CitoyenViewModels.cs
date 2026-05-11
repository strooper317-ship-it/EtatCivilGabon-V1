using EtatCivilGabon.Models;
using System.ComponentModel.DataAnnotations;

namespace EtatCivilGabon.ViewModels
{
    // ── Nouvelle demande ─────────────────────────────────────────────────────
    public class NouvelleDemandViewModel
    {
        [Required(ErrorMessage = "Veuillez choisir un type d'acte.")]
        [Display(Name = "Type d'acte")]
        public int TypeActeId { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [MaxLength(100)]
        [Display(Name = "Nom")]
        public string NomDemandeur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [MaxLength(100)]
        [Display(Name = "Prénom")]
        public string PrenomDemandeur { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'objet de la demande est obligatoire.")]
        [MaxLength(200)]
        [Display(Name = "Objet de la demande")]
        public string ObjetDemande { get; set; } = string.Empty;

        [Display(Name = "Pièces justificatives")]
        public List<IFormFile> Fichiers { get; set; } = new();

        public List<TypeActe> TypesActes { get; set; } = new();
    }

    // ── Détail d'une demande ─────────────────────────────────────────────────
    public class DetailDemandeViewModel
    {
        public int    Id               { get; set; }
        public string NumeroSuivi      { get; set; } = string.Empty;
        public string TypeActeLibelle  { get; set; } = string.Empty;
        public string NomDemandeur     { get; set; } = string.Empty;
        public string PrenomDemandeur  { get; set; } = string.Empty;
        public string ObjetDemande     { get; set; } = string.Empty;
        public StatutDemande Statut    { get; set; }
        public DateTime  DateSoumission  { get; set; }
        public DateTime? DateMiseAJour   { get; set; }
        public string?   CommentaireAgent{ get; set; }
        public List<PieceJointe> PiecesJointes { get; set; } = new();
        public int DelaiCible { get; set; }

        // Document final envoyé par l'agent
        public string?   NomDocumentFinal    { get; set; }
        public DateTime? DateEnvoiDocument   { get; set; }
    }

    // ── Liste des demandes du citoyen ────────────────────────────────────────
    public class MesDemandesViewModel
    {
        public List<Demande> Demandes  { get; set; } = new();
        public int TotalEnAttente      { get; set; }
        public int TotalEnCours        { get; set; }
        public int TotalPret           { get; set; }
        public int TotalRejete         { get; set; }
    }

    // ── Suivi public par numéro ──────────────────────────────────────────────
    public class SuiviPublicViewModel
    {
        [Required(ErrorMessage = "Le numéro de suivi est obligatoire.")]
        [Display(Name = "Numéro de suivi")]
        [RegularExpression(@"GAB-\d{4}-\d{5}",
            ErrorMessage = "Format attendu : GAB-AAAA-NNNNN (ex: GAB-2024-00001)")]
        public string NumeroSuivi { get; set; } = string.Empty;

        public DetailDemandeViewModel? Resultat    { get; set; }
        public bool RechercheEffectuee             { get; set; }
    }
}
