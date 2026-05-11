using EtatCivilGabon.Models;

namespace EtatCivilGabon.Services
{
    public interface IDemandeService
    {
        // Citoyen
        Task<Demande> CreerDemandeAsync(Demande demande, List<IFormFile> fichiers, string citoyenId);
        Task<Demande?> ObtenirParNumeroSuiviAsync(string numeroSuivi);
        Task<IEnumerable<Demande>> ObtenirDemandesCitoyenAsync(string citoyenId);

        // Agent
        Task<IEnumerable<Demande>> ObtenirToutesLesDemandesAsync();
        Task<Demande?> ObtenirParIdAsync(int id);
        Task MettreAJourStatutAsync(
            int id,
            StatutDemande statut,
            string commentaire,
            string? cheminDocumentFinal  = null,
            string? nomDocumentFinal     = null,
            string? typeMimeDocument     = null,
            long?   tailleDocument       = null);

        // Admin
        Task<Dictionary<string, int>> ObtenirStatistiquesAsync();
    }
}
