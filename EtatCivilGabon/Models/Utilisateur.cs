using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EtatCivilGabon.Models
{
    // Étend IdentityUser pour ajouter les propriétés métier
    public class Utilisateur : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Display(Name = "Date d'inscription")]
        public DateTime DateInscription { get; set; } = DateTime.Now;

        [Display(Name = "Compte actif")]
        public bool EstActif { get; set; } = true;

        // Nom complet calculé
        public string NomComplet => $"{Prenom} {Nom}";
    }
}
