using EtatCivilGabon.Models;
using EtatCivilGabon.Services;
using EtatCivilGabon.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace EtatCivilGabon.Tests.Services
{
    public class DemandeServiceTests
    {
        private DemandeService CreerService(string nomBase = "")
        {
            var context     = TestDbHelper.CreerContexteEnMemoire(nomBase);
            var generator   = new NumeroSuiviGenerator(context);
            var envMock     = new Mock<IWebHostEnvironment>();
            var loggerMock  = new Mock<ILogger<DemandeService>>();

            envMock.Setup(e => e.WebRootPath)
                   .Returns(Path.GetTempPath());

            return new DemandeService(context, generator, envMock.Object, loggerMock.Object);
        }

        // ── Créer une demande ────────────────────────────────────────────────

        [Fact]
        public async Task CreerDemande_AssigneNumeroSuivi()
        {
            var service = CreerService();
            var demande = TestDbHelper.CreerDemandeTest();
            demande.NumeroSuivi = string.Empty;

            var creee = await service.CreerDemandeAsync(demande, new List<IFormFile>(), "citoyen-1");

            creee.NumeroSuivi.Should().MatchRegex(@"^GAB-\d{4}-\d{5}$");
        }

        [Fact]
        public async Task CreerDemande_StatutInitialEstEnAttente()
        {
            var service = CreerService();
            var demande = TestDbHelper.CreerDemandeTest();

            var creee = await service.CreerDemandeAsync(demande, new List<IFormFile>(), "citoyen-1");

            creee.Statut.Should().Be(StatutDemande.EnAttente);
        }

        [Fact]
        public async Task CreerDemande_AssigneLeCitoyenId()
        {
            var service    = CreerService();
            var demande    = TestDbHelper.CreerDemandeTest();
            const string id = "citoyen-xyz-123";

            var creee = await service.CreerDemandeAsync(demande, new List<IFormFile>(), id);

            creee.CitoyenId.Should().Be(id);
        }

        [Fact]
        public async Task CreerDemande_DateSoumissionEstDefinie()
        {
            var service = CreerService();
            var demande = TestDbHelper.CreerDemandeTest();
            var avant   = DateTime.Now.AddSeconds(-1);

            var creee = await service.CreerDemandeAsync(demande, new List<IFormFile>(), "c1");

            creee.DateSoumission.Should().BeAfter(avant);
        }

        // ── Obtenir par numéro de suivi ──────────────────────────────────────

        [Fact]
        public async Task ObtenirParNumeroSuivi_RetourneDemande_SiExiste()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire("suivi-test");
            var generator = new NumeroSuiviGenerator(context);
            var envMock   = new Mock<IWebHostEnvironment>();
            var logMock   = new Mock<ILogger<DemandeService>>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            var service = new DemandeService(context, generator, envMock.Object, logMock.Object);
            var demande = TestDbHelper.CreerDemandeTest();
            var creee   = await service.CreerDemandeAsync(demande, new List<IFormFile>(), "c1");

            var trouve = await service.ObtenirParNumeroSuiviAsync(creee.NumeroSuivi);

            trouve.Should().NotBeNull();
            trouve!.NumeroSuivi.Should().Be(creee.NumeroSuivi);
        }

        [Fact]
        public async Task ObtenirParNumeroSuivi_RetourneNull_SiInexistant()
        {
            var service = CreerService("suivi-null");

            var resultat = await service.ObtenirParNumeroSuiviAsync("GAB-9999-99999");

            resultat.Should().BeNull();
        }

        // ── Obtenir par citoyen ──────────────────────────────────────────────

        [Fact]
        public async Task ObtenirDemandesCitoyen_RetourneSeulementSesdemandes()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire("citoyen-filter");
            var generator = new NumeroSuiviGenerator(context);
            var envMock   = new Mock<IWebHostEnvironment>();
            var logMock   = new Mock<ILogger<DemandeService>>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            var service   = new DemandeService(context, generator, envMock.Object, logMock.Object);

            await service.CreerDemandeAsync(TestDbHelper.CreerDemandeTest(), new List<IFormFile>(), "citoyenA");
            await service.CreerDemandeAsync(TestDbHelper.CreerDemandeTest(), new List<IFormFile>(), "citoyenA");
            await service.CreerDemandeAsync(TestDbHelper.CreerDemandeTest(), new List<IFormFile>(), "citoyenB");

            var demandesA = (await service.ObtenirDemandesCitoyenAsync("citoyenA")).ToList();

            demandesA.Should().HaveCount(2);
            demandesA.Should().OnlyContain(d => d.CitoyenId == "citoyenA");
        }

        // ── Mettre à jour le statut ──────────────────────────────────────────

        [Fact]
        public async Task MettreAJourStatut_ModifieLeDossier()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire("statut-test");
            var generator = new NumeroSuiviGenerator(context);
            var envMock   = new Mock<IWebHostEnvironment>();
            var logMock   = new Mock<ILogger<DemandeService>>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            var service   = new DemandeService(context, generator, envMock.Object, logMock.Object);

            var creee = await service.CreerDemandeAsync(
                TestDbHelper.CreerDemandeTest(), new List<IFormFile>(), "c1");

            await service.MettreAJourStatutAsync(
                creee.Id, StatutDemande.Pret, "Document prêt à être retiré.");

            var mise_a_jour = await service.ObtenirParIdAsync(creee.Id);

            mise_a_jour!.Statut.Should().Be(StatutDemande.Pret);
            mise_a_jour.CommentaireAgent.Should().Be("Document prêt à être retiré.");
            mise_a_jour.DateMiseAJour.Should().NotBeNull();
        }

        [Fact]
        public async Task MettreAJourStatut_AvecStatutRejete_EnregistreLeCommentaire()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire("rejete-test");
            var generator = new NumeroSuiviGenerator(context);
            var envMock   = new Mock<IWebHostEnvironment>();
            var logMock   = new Mock<ILogger<DemandeService>>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            var service   = new DemandeService(context, generator, envMock.Object, logMock.Object);

            var creee = await service.CreerDemandeAsync(
                TestDbHelper.CreerDemandeTest(), new List<IFormFile>(), "c1");

            const string raison = "Pièce d'identité manquante.";
            await service.MettreAJourStatutAsync(creee.Id, StatutDemande.Rejete, raison);

            var d = await service.ObtenirParIdAsync(creee.Id);
            d!.Statut.Should().Be(StatutDemande.Rejete);
            d.CommentaireAgent.Should().Be(raison);
        }

        // ── Statistiques ─────────────────────────────────────────────────────

        [Fact]
        public async Task ObtenirStatistiques_RetourneComptesCorrects()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire("stats-test");
            var generator = new NumeroSuiviGenerator(context);
            var envMock   = new Mock<IWebHostEnvironment>();
            var logMock   = new Mock<ILogger<DemandeService>>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            var service   = new DemandeService(context, generator, envMock.Object, logMock.Object);

            await service.CreerDemandeAsync(TestDbHelper.CreerDemandeTest(typeActeId: 1),
                new List<IFormFile>(), "c1");
            await service.CreerDemandeAsync(TestDbHelper.CreerDemandeTest(typeActeId: 2),
                new List<IFormFile>(), "c2");
            await service.CreerDemandeAsync(TestDbHelper.CreerDemandeTest(typeActeId: 1),
                new List<IFormFile>(), "c3");

            var stats = await service.ObtenirStatistiquesAsync();

            stats["TotalDemandes"].Should().Be(3);
            stats["EnAttente"].Should().Be(3);
            stats["ActeNaissance"].Should().Be(2);
            stats["ActeMariage"].Should().Be(1);
        }
    }
}
