using EtatCivilGabon.Models;

namespace EtatCivilGabon.Services
{
    /// <summary>
    /// Contrat du service de notifications par e-mail.
    /// Implémenté en Phase 5 avec MailKit.
    /// </summary>
    public interface INotificationService
    {
        Task EnvoyerConfirmationDemandeAsync(Demande demande, string emailCitoyen);
        Task EnvoyerChangementStatutAsync(Demande demande, string emailCitoyen);
        Task EnvoyerDocumentPretAsync(Demande demande, string emailCitoyen);
    }
}
