using EtatCivilGabon.Models;
using EtatCivilGabon.Tests.Helpers;
using FluentAssertions;

namespace EtatCivilGabon.Tests.Data
{
    public class AppDbContextTests
    {
        // ── Opérations CRUD de base ──────────────────────────────────────────

        [Fact]
        public async Task Demande_PeutEtreAjoutee()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            var demande = TestDbHelper.CreerDemandeTest();

            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            context.Demandes.Should().HaveCount(1);
        }

        [Fact]
        public async Task Demande_PeutEtreLue()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            var demande = TestDbHelper.CreerDemandeTest();
            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            var lue = context.Demandes.Find(demande.Id);

            lue.Should().NotBeNull();
            lue!.NomDemandeur.Should().Be("OBAME");
        }

        [Fact]
        public async Task Demande_PeutEtreModifiee()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            var demande = TestDbHelper.CreerDemandeTest();
            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            demande.Statut = StatutDemande.EnCours;
            await context.SaveChangesAsync();

            var modifiee = context.Demandes.Find(demande.Id);
            modifiee!.Statut.Should().Be(StatutDemande.EnCours);
        }

        [Fact]
        public async Task Demande_PeutEtreSupprimee()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            var demande = TestDbHelper.CreerDemandeTest();
            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            context.Demandes.Remove(demande);
            await context.SaveChangesAsync();

            context.Demandes.Should().BeEmpty();
        }

        // ── Relations ────────────────────────────────────────────────────────

        [Fact]
        public async Task PieceJointe_CascadeSuppressionAvecDemande()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            var demande = TestDbHelper.CreerDemandeTest();
            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            context.PiecesJointes.Add(new PieceJointe
            {
                DemandeId     = demande.Id,
                NomFichier    = "piece.pdf",
                CheminFichier = "/tmp/piece.pdf",
                TypeMime      = "application/pdf",
                Taille        = 1024,
            });
            await context.SaveChangesAsync();

            context.Demandes.Remove(demande);
            await context.SaveChangesAsync();

            context.PiecesJointes.Should().BeEmpty(
                "les pièces jointes doivent être supprimées en cascade");
        }

        [Fact]
        public async Task Notification_CascadeSuppressionAvecDemande()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            var demande = TestDbHelper.CreerDemandeTest();
            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            context.Notifications.Add(new Notification
            {
                DemandeId         = demande.Id,
                Message           = "Confirmation de dépôt",
                EmailDestinataire = "test@test.ga",
                EnvoyeeAvecSucces = true,
            });
            await context.SaveChangesAsync();

            context.Demandes.Remove(demande);
            await context.SaveChangesAsync();

            context.Notifications.Should().BeEmpty(
                "les notifications doivent être supprimées en cascade");
        }

        // ── Index unique ─────────────────────────────────────────────────────

        [Fact]
        public async Task TypeActe_CodeUnique_LeveeExceptionSiDoublon()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();

            context.TypesActes.Add(new TypeActe
            {
                Id = 100, Code = "ACT-TST", Libelle = "Test 1",
                DelaiCible = 3, EstActif = true
            });
            await context.SaveChangesAsync();

            context.TypesActes.Add(new TypeActe
            {
                Id = 101, Code = "ACT-TST", Libelle = "Test 2 doublon",
                DelaiCible = 3, EstActif = true
            });

            var action = async () => await context.SaveChangesAsync();
            await action.Should().ThrowAsync<Exception>(
                "un code TypeActe dupliqué doit lever une exception");
        }
    }
}
