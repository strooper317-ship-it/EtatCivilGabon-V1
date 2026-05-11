using EtatCivilGabon.Models;
using EtatCivilGabon.Tests.Helpers;
using FluentAssertions;

namespace EtatCivilGabon.Tests.Models
{
    public class ModeleTests
    {
        // ── Utilisateur ──────────────────────────────────────────────────────

        [Fact]
        public void Utilisateur_NomComplet_CombinePrenoNom()
        {
            var u = new Utilisateur { Prenom = "Jean", Nom = "OBAME" };
            u.NomComplet.Should().Be("Jean OBAME");
        }

        [Fact]
        public void Utilisateur_NouveauCompte_EstActifParDefaut()
        {
            var u = new Utilisateur();
            u.EstActif.Should().BeTrue();
        }

        // ── Demande ──────────────────────────────────────────────────────────

        [Fact]
        public void Demande_NouvelleInstance_StatutEnAttente()
        {
            var d = new Demande();
            d.Statut.Should().Be(StatutDemande.EnAttente);
        }

        [Fact]
        public void Demande_NouvelleInstance_PiecesJointesVide()
        {
            var d = new Demande();
            d.PiecesJointes.Should().NotBeNull();
            d.PiecesJointes.Should().BeEmpty();
        }

        [Fact]
        public void Demande_NouvelleInstance_NotificationsVide()
        {
            var d = new Demande();
            d.Notifications.Should().NotBeNull();
            d.Notifications.Should().BeEmpty();
        }

        // ── TypeActe ─────────────────────────────────────────────────────────

        [Fact]
        public void TypeActe_NouvelleInstance_EstActifParDefaut()
        {
            var t = new TypeActe();
            t.EstActif.Should().BeTrue();
        }

        [Fact]
        public void TypeActe_DemandesNavigation_NonNulle()
        {
            var t = new TypeActe();
            t.Demandes.Should().NotBeNull();
        }

        // ── Enums ────────────────────────────────────────────────────────────

        [Fact]
        public void StatutDemande_ContientTousLesStatuts()
        {
            var valeurs = Enum.GetValues<StatutDemande>();
            valeurs.Should().Contain(StatutDemande.EnAttente);
            valeurs.Should().Contain(StatutDemande.EnCours);
            valeurs.Should().Contain(StatutDemande.Pret);
            valeurs.Should().Contain(StatutDemande.Rejete);
        }

        [Fact]
        public void RoleUtilisateur_ContientTroisRoles()
        {
            var valeurs = Enum.GetValues<RoleUtilisateur>();
            valeurs.Should().HaveCount(3);
            valeurs.Should().Contain(RoleUtilisateur.Citoyen);
            valeurs.Should().Contain(RoleUtilisateur.Agent);
            valeurs.Should().Contain(RoleUtilisateur.Administrateur);
        }

        // ── Seed des types d'actes ────────────────────────────────────────────

        [Fact]
        public void SeedTypesActes_ContientTroisEntrees()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            context.TypesActes.Should().HaveCount(3);
        }

        [Fact]
        public void SeedTypesActes_CodesCorrects()
        {
            var context = TestDbHelper.CreerContexteEnMemoire();
            var codes   = context.TypesActes.Select(t => t.Code).ToList();
            codes.Should().Contain("ACT-NAI");
            codes.Should().Contain("ACT-MAR");
            codes.Should().Contain("ACT-DEC");
        }

        [Fact]
        public void SeedTypesActes_DelaisCorrects()
        {
            var context   = TestDbHelper.CreerContexteEnMemoire();
            var naissance = context.TypesActes.First(t => t.Code == "ACT-NAI");
            var mariage   = context.TypesActes.First(t => t.Code == "ACT-MAR");
            var deces     = context.TypesActes.First(t => t.Code == "ACT-DEC");

            naissance.DelaiCible.Should().Be(5);
            mariage.DelaiCible.Should().Be(7);
            deces.DelaiCible.Should().Be(5);
        }
    }
}
