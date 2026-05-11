using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtatCivilGabon.Models
{
    public class PieceJointe
    {
        public int Id { get; set; }

        [Required]
        public int DemandeId { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Nom du fichier")]
        public string NomFichier { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Display(Name = "Chemin du fichier")]
        public string CheminFichier { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Type MIME")]
        public string TypeMime { get; set; } = string.Empty;

        [Display(Name = "Taille (octets)")]
        public long Taille { get; set; }

        [Display(Name = "Date de dépôt")]
        public DateTime DateDepot { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("DemandeId")]
        public Demande? Demande { get; set; }
    }
}
