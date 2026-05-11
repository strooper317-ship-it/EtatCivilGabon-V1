using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtatCivilGabon.Models
{
    public class Demande
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Numéro de suivi")]
        public string NumeroSuivi { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Type d'acte")]
        public int TypeActeId { get; set; }

        [Required]
        [Display(Name = "Citoyen")]
        public string CitoyenId { get; set; } = string.Empty;

        [Display(Name = "Statut")]
        public StatutDemande Statut { get; set; } = StatutDemande.EnAttente;

        [Display(Name = "Date de soumission")]
        public DateTime DateSoumission { get; set; } = DateTime.Now;

        [Display(Name = "Date de dernière mise à jour")]
        public DateTime? DateMiseAJour { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Commentaire de l'agent")]
        public string? CommentaireAgent { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nom du demandeur")]
        public string NomDemandeur { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Prénom du demandeur")]
        public string PrenomDemandeur { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Display(Name = "Objet de la demande")]
        public string ObjetDemande { get; set; } = string.Empty;

        // ── Document final envoyé par l'agent ────────────────────────────────
        [MaxLength(255)]
        [Display(Name = "Nom du document final")]
        public string? NomDocumentFinal { get; set; }

        [MaxLength(500)]
        [Display(Name = "Chemin du document final")]
        public string? CheminDocumentFinal { get; set; }

        [MaxLength(100)]
        public string? TypeMimeDocumentFinal { get; set; }

        public long? TailleDocumentFinal { get; set; }

        [Display(Name = "Date d'envoi du document")]
        public DateTime? DateEnvoiDocument { get; set; }

        // Navigation
        [ForeignKey("TypeActeId")]
        public TypeActe? TypeActe { get; set; }

        public ICollection<PieceJointe>  PiecesJointes { get; set; } = new List<PieceJointe>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
