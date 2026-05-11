using EtatCivilGabon.Models;
using System.ComponentModel.DataAnnotations;

namespace EtatCivilGabon.ViewModels
{
    // ── Tableau de bord admin ────────────────────────────────────────────────
    public class TableauDeBordAdminViewModel
    {
        public int TotalDemandes       { get; set; }
        public int TotalEnAttente      { get; set; }
        public int TotalEnCours        { get; set; }
        public int TotalPret           { get; set; }
        public int TotalRejete         { get; set; }
        public int TotalActeNaissance  { get; set; }
        public int TotalActeMariage    { get; set; }
        public int TotalActeDeces      { get; set; }
        public double DelaiMoyenTraitement { get; set; }
        public double TauxRejet            { get; set; }
        public int NombreAgents            { get; set; }
        public List<Demande>       DernieresDemandes { get; set; } = new();
        public List<StatMensuelle> StatsMensuelles   { get; set; } = new();
    }

    public class StatMensuelle
    {
        public string Mois   { get; set; } = string.Empty;
        public int    Nombre { get; set; }
    }

    // ── Gestion des agents ───────────────────────────────────────────────────
    public class GestionAgentsViewModel
    {
        public List<AgentViewModel> Agents { get; set; } = new();
    }

    public class AgentViewModel
    {
        public string   Id               { get; set; } = string.Empty;
        public string   NomComplet       { get; set; } = string.Empty;
        public string   Email            { get; set; } = string.Empty;
        public bool     EstActif         { get; set; }
        public DateTime DateInscription  { get; set; }
        public int      NombreDossiers   { get; set; }
    }

    // ── Créer un agent ───────────────────────────────────────────────────────
    public class CreerAgentViewModel
    {
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [MaxLength(100)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [MaxLength(100)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [MinLength(8, ErrorMessage = "Minimum 8 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe temporaire")]
        public string MotDePasse { get; set; } = string.Empty;
    }
}
