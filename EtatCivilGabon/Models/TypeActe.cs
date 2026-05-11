using System.ComponentModel.DataAnnotations;

namespace EtatCivilGabon.Models
{
    public class TypeActe
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Libellé")]
        public string Libelle { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty; // ex: ACT-NAI

        [Display(Name = "Délai cible (jours ouvrés)")]
        public int DelaiCible { get; set; }

        [MaxLength(500)]
        [Display(Name = "Pièces requises")]
        public string PiecesRequises { get; set; } = string.Empty;

        public bool EstActif { get; set; } = true;

        // Navigation
        public ICollection<Demande> Demandes { get; set; } = new List<Demande>();
    }
}
