using EtatCivilGabon.Services;
using EtatCivilGabon.Tests.Helpers;
using FluentAssertions;

namespace EtatCivilGabon.Tests.Services
{
    public class NumeroSuiviGeneratorTests
    {
        // ── Format du numéro ─────────────────────────────────────────────────

        [Fact]
        public async Task Generer_RetourneFormatCorrect()
        {
            // Arrange
            var context   = TestDbHelper.CreerContexteEnMemoire();
            var generator = new NumeroSuiviGenerator(context);

            // Act
            var numero = await generator.GenererAsync();

            // Assert
            numero.Should().MatchRegex(@"^GAB-\d{4}-\d{5}$",
                "le numéro doit respecter le format GAB-AAAA-NNNNN");
        }

        [Fact]
        public async Task Generer_ContientAnneeActuelle()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire();
            var generator = new NumeroSuiviGenerator(context);

            var numero = await generator.GenererAsync();

            numero.Should().Contain(DateTime.Now.Year.ToString());
        }

        // ── Unicité ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Generer_DeuxAppels_ProduisentNumerosDifferents()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire();
            var generator = new NumeroSuiviGenerator(context);

            // Simuler une première demande déjà en base
            var demande = TestDbHelper.CreerDemandeTest();
            demande.NumeroSuivi = await generator.GenererAsync();
            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            // Générer un second numéro
            var second = await generator.GenererAsync();

            second.Should().NotBe(demande.NumeroSuivi,
                "deux appels successifs doivent produire des numéros uniques");
        }

        [Fact]
        public async Task Generer_PremierNumero_FinitParUn()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire();
            var generator = new NumeroSuiviGenerator(context);

            var numero = await generator.GenererAsync();

            numero.Should().EndWith("00001",
                "le premier numéro de l'année doit se terminer par 00001");
        }

        // ── Incrément ────────────────────────────────────────────────────────

        [Fact]
        public async Task Generer_ApresUneDemande_IncrementeLeCompteur()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire();
            var generator = new NumeroSuiviGenerator(context);

            var premier = await generator.GenererAsync();
            var demande = TestDbHelper.CreerDemandeTest();
            demande.NumeroSuivi = premier;
            context.Demandes.Add(demande);
            await context.SaveChangesAsync();

            var second = await generator.GenererAsync();

            second.Should().EndWith("00002",
                "le deuxième numéro doit se terminer par 00002");
        }
    }
}
