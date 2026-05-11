using EtatCivilGabon.Data;
using Microsoft.EntityFrameworkCore;

namespace EtatCivilGabon.Services
{
    /// <summary>
    /// Génère un numéro de suivi unique au format GAB-AAAA-NNNNN
    /// Exemple : GAB-2024-00123
    /// </summary>
    public class NumeroSuiviGenerator
    {
        private readonly AppDbContext _context;
        private static readonly SemaphoreSlim _verrou = new SemaphoreSlim(1, 1);

        public NumeroSuiviGenerator(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenererAsync()
        {
            await _verrou.WaitAsync();
            try
            {
                var annee = DateTime.Now.Year;

                // Compte le nombre de demandes déjà créées cette année
                var compteur = await _context.Demandes
                    .CountAsync(d => d.DateSoumission.Year == annee);

                var numero = $"GAB-{annee}-{(compteur + 1):D5}";

                // Sécurité : si le numéro existe déjà, on incrémente
                while (await _context.Demandes.AnyAsync(d => d.NumeroSuivi == numero))
                {
                    compteur++;
                    numero = $"GAB-{annee}-{(compteur + 1):D5}";
                }

                return numero;
            }
            finally
            {
                _verrou.Release();
            }
        }
    }
}
