using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EtatCivilGabon.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int DemandeId { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Date d'envoi")]
        public DateTime DateEnvoi { get; set; } = DateTime.Now;

        [Display(Name = "Envoyée avec succès")]
        public bool EnvoyeeAvecSucces { get; set; } = false;

        [MaxLength(255)]
        [Display(Name = "Email destinataire")]
        public string EmailDestinataire { get; set; } = string.Empty;

        // Navigation
        [ForeignKey("DemandeId")]
        public Demande? Demande { get; set; }
    }
}
