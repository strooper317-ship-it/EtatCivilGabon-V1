using EtatCivilGabon.ViewModels;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace EtatCivilGabon.Tests.ViewModels
{
    public class ViewModelValidationTests
    {
        // ── Helper validation ────────────────────────────────────────────────
        private static List<ValidationResult> Valider(object model)
        {
            var resultats = new List<ValidationResult>();
            var contexte  = new ValidationContext(model);
            Validator.TryValidateObject(model, contexte, resultats, true);
            return resultats;
        }

        // ── InscriptionViewModel ─────────────────────────────────────────────

        [Fact]
        public void Inscription_ModeleValide_AucuneErreur()
        {
            var vm = new InscriptionViewModel
            {
                Nom                   = "OBAME",
                Prenom                = "Jean",
                Email                 = "jean@test.ga",
                MotDePasse            = "Gabon@2024!",
                ConfirmationMotDePasse= "Gabon@2024!"
            };
            Valider(vm).Should().BeEmpty();
        }

        [Fact]
        public void Inscription_EmailInvalide_ProduiteErreur()
        {
            var vm = new InscriptionViewModel
            {
                Nom                   = "OBAME",
                Prenom                = "Jean",
                Email                 = "email-invalide",
                MotDePasse            = "Gabon@2024!",
                ConfirmationMotDePasse= "Gabon@2024!"
            };
            var erreurs = Valider(vm);
            erreurs.Should().Contain(e =>
                e.MemberNames.Contains("Email"));
        }

        [Fact]
        public void Inscription_NomManquant_ProduiteErreur()
        {
            var vm = new InscriptionViewModel
            {
                Nom                   = "",
                Prenom                = "Jean",
                Email                 = "jean@test.ga",
                MotDePasse            = "Gabon@2024!",
                ConfirmationMotDePasse= "Gabon@2024!"
            };
            var erreurs = Valider(vm);
            erreurs.Should().Contain(e =>
                e.MemberNames.Contains("Nom"));
        }

        [Fact]
        public void Inscription_MotDePasseTropCourt_ProduiteErreur()
        {
            var vm = new InscriptionViewModel
            {
                Nom                   = "OBAME",
                Prenom                = "Jean",
                Email                 = "jean@test.ga",
                MotDePasse            = "abc",
                ConfirmationMotDePasse= "abc"
            };
            var erreurs = Valider(vm);
            erreurs.Should().Contain(e =>
                e.MemberNames.Contains("MotDePasse"));
        }

        // ── ConnexionViewModel ───────────────────────────────────────────────

        [Fact]
        public void Connexion_ModeleValide_AucuneErreur()
        {
            var vm = new ConnexionViewModel
            {
                Email      = "jean@test.ga",
                MotDePasse = "Gabon@2024!"
            };
            Valider(vm).Should().BeEmpty();
        }

        [Fact]
        public void Connexion_EmailVide_ProduiteErreur()
        {
            var vm = new ConnexionViewModel
            {
                Email      = "",
                MotDePasse = "Gabon@2024!"
            };
            var erreurs = Valider(vm);
            erreurs.Should().Contain(e =>
                e.MemberNames.Contains("Email"));
        }

        // ── NouvelleDemandViewModel ──────────────────────────────────────────

        [Fact]
        public void NouvelleDemande_ModeleValide_AucuneErreur()
        {
            var vm = new NouvelleDemandViewModel
            {
                TypeActeId      = 1,
                NomDemandeur    = "OBAME",
                PrenomDemandeur = "Jean",
                ObjetDemande    = "Obtention acte de naissance"
            };
            Valider(vm).Should().BeEmpty();
        }

        [Fact]
        public void NouvelleDemande_TypeActeManquant_ProduiteErreur()
        {
            var vm = new NouvelleDemandViewModel
            {
                TypeActeId      = 0,
                NomDemandeur    = "OBAME",
                PrenomDemandeur = "Jean",
                ObjetDemande    = "Test"
            };
            var erreurs = Valider(vm);
            erreurs.Should().Contain(e =>
                e.MemberNames.Contains("TypeActeId"));
        }

        // ── SuiviPublicViewModel ─────────────────────────────────────────────

        [Fact]
        public void SuiviPublic_FormatCorrect_AucuneErreur()
        {
            var vm = new SuiviPublicViewModel
            {
                NumeroSuivi = "GAB-2024-00123"
            };
            Valider(vm).Should().BeEmpty();
        }

        [Fact]
        public void SuiviPublic_FormatIncorrect_ProduiteErreur()
        {
            var vm = new SuiviPublicViewModel
            {
                NumeroSuivi = "MAUVAIS-FORMAT"
            };
            var erreurs = Valider(vm);
            erreurs.Should().Contain(e =>
                e.MemberNames.Contains("NumeroSuivi"));
        }

        // ── TraiterDemandeViewModel ──────────────────────────────────────────

        [Fact]
        public void TraiterDemande_AvecCommentaire_AucuneErreur()
        {
            var vm = new TraiterDemandeViewModel
            {
                Id            = 1,
                NumeroSuivi   = "GAB-2024-00001",
                NouveauStatut = EtatCivilGabon.Models.StatutDemande.Pret,
                Commentaire   = "Document prêt."
            };
            Valider(vm).Should().BeEmpty();
        }

        // ── CreerAgentViewModel ──────────────────────────────────────────────

        [Fact]
        public void CreerAgent_ModeleValide_AucuneErreur()
        {
            var vm = new CreerAgentViewModel
            {
                Nom       = "MBADINGA",
                Prenom    = "Jean",
                Email     = "agent@mairie.ga",
                MotDePasse= "Agent@2024!"
            };
            Valider(vm).Should().BeEmpty();
        }

        [Fact]
        public void CreerAgent_EmailInvalide_ProduiteErreur()
        {
            var vm = new CreerAgentViewModel
            {
                Nom       = "MBADINGA",
                Prenom    = "Jean",
                Email     = "pas-un-email",
                MotDePasse= "Agent@2024!"
            };
            var erreurs = Valider(vm);
            erreurs.Should().Contain(e =>
                e.MemberNames.Contains("Email"));
        }
    }
}
