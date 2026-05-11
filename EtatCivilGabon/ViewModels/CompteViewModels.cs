using System.ComponentModel.DataAnnotations;

namespace EtatCivilGabon.ViewModels
{
    // ── Inscription ──────────────────────────────────────────────────────────
    public class InscriptionViewModel
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
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation est obligatoire.")]
        [DataType(DataType.Password)]
        [Compare("MotDePasse", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmationMotDePasse { get; set; } = string.Empty;
    }

    // ── Connexion ────────────────────────────────────────────────────────────
    public class ConnexionViewModel
    {
        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; } = string.Empty;

        [Display(Name = "Se souvenir de moi")]
        public bool Sesouvenir { get; set; }
    }

    // ── Changement de mot de passe ───────────────────────────────────────────
    public class ChangerMotDePasseViewModel
    {
        [Required(ErrorMessage = "L'ancien mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Ancien mot de passe")]
        public string AncienMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation est obligatoire.")]
        [DataType(DataType.Password)]
        [Compare("NouveauMotDePasse", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        public string ConfirmationMotDePasse { get; set; } = string.Empty;
    }
}
