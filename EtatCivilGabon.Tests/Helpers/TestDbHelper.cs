using EtatCivilGabon.Data;
using EtatCivilGabon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtatCivilGabon.Tests.Helpers
{
    public static class TestDbHelper
    {
        /// <summary>
        /// Crée un AppDbContext en mémoire isolé pour chaque test.
        /// </summary>
        public static AppDbContext CreerContexteEnMemoire(string nomBase = "")
        {
            if (string.IsNullOrEmpty(nomBase))
                nomBase = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(nomBase)
                .Options;

            var context = new AppDbContext(options);

            // Seed des types d'actes (identique à la migration)
            if (!context.TypesActes.Any())
            {
                context.TypesActes.AddRange(
                    new TypeActe
                    {
                        Id = 1, Code = "ACT-NAI", Libelle = "Acte de naissance",
                        DelaiCible = 5,
                        PiecesRequises = "Pièce d'identité du déclarant",
                        EstActif = true
                    },
                    new TypeActe
                    {
                        Id = 2, Code = "ACT-MAR", Libelle = "Acte de mariage",
                        DelaiCible = 7,
                        PiecesRequises = "Pièces d'identité des deux époux",
                        EstActif = true
                    },
                    new TypeActe
                    {
                        Id = 3, Code = "ACT-DEC", Libelle = "Acte de décès",
                        DelaiCible = 5,
                        PiecesRequises = "Pièce d'identité du déclarant",
                        EstActif = true
                    }
                );
                context.SaveChanges();
            }

            return context;
        }

        /// <summary>
        /// Crée une demande de test avec toutes les propriétés minimales.
        /// </summary>
        public static Demande CreerDemandeTest(
            int typeActeId      = 1,
            string citoyenId    = "citoyen-test-001",
            StatutDemande statut = StatutDemande.EnAttente)
        {
            return new Demande
            {
                TypeActeId      = typeActeId,
                CitoyenId       = citoyenId,
                NumeroSuivi     = $"GAB-{DateTime.Now.Year}-00001",
                Statut          = statut,
                NomDemandeur    = "OBAME",
                PrenomDemandeur = "Jean-Pierre",
                ObjetDemande    = "Obtention acte de naissance",
                DateSoumission  = DateTime.Now,
            };
        }
    }
}
