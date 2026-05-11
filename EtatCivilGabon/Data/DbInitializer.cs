using EtatCivilGabon.Models;
using Microsoft.AspNetCore.Identity;

namespace EtatCivilGabon.Data
{
    public static class DbInitializer
    {
        // Crée les rôles et un compte admin par défaut au premier démarrage
        public static async Task InitialiserAsync(
            IServiceProvider services,
            UserManager<Utilisateur> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ── 1. Créer les rôles ───────────────────────────────────────────
            string[] roles = { "Administrateur", "Agent", "Citoyen" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ── 2. Créer le compte Administrateur par défaut ─────────────────
            const string adminEmail = "admin@etatcivil.ga";
            const string adminPassword = "Admin@2024!";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new Utilisateur
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nom = "Administrateur",
                    Prenom = "Système",
                    EmailConfirmed = true,
                    EstActif = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrateur");
                }
            }

            // ── 3. Créer un agent de test ────────────────────────────────────
            const string agentEmail = "agent@etatcivil.ga";
            const string agentPassword = "Agent@2024!";

            if (await userManager.FindByEmailAsync(agentEmail) == null)
            {
                var agent = new Utilisateur
                {
                    UserName = agentEmail,
                    Email = agentEmail,
                    Nom = "Mbadinga",
                    Prenom = "Jean",
                    EmailConfirmed = true,
                    EstActif = true
                };

                var result = await userManager.CreateAsync(agent, agentPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(agent, "Agent");
                }
            }
        }
    }
}
