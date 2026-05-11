using EtatCivilGabon.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtatCivilGabon.Data
{
    public class AppDbContext : IdentityDbContext<Utilisateur>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Demande> Demandes { get; set; }
        public DbSet<TypeActe> TypesActes { get; set; }
        public DbSet<PieceJointe> PiecesJointes { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Configuration Demande ────────────────────────────────────────
            builder.Entity<Demande>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.HasIndex(d => d.NumeroSuivi).IsUnique();

                entity.Property(d => d.Statut)
                      .HasConversion<string>(); // stocke l'enum en string

                entity.HasOne(d => d.TypeActe)
                      .WithMany(t => t.Demandes)
                      .HasForeignKey(d => d.TypeActeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.PiecesJointes)
                      .WithOne(p => p.Demande)
                      .HasForeignKey(p => p.DemandeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(d => d.Notifications)
                      .WithOne(n => n.Demande)
                      .HasForeignKey(n => n.DemandeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Configuration TypeActe ───────────────────────────────────────
            builder.Entity<TypeActe>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasIndex(t => t.Code).IsUnique();
            });

            // ── Seed : Types d'actes (selon cahier des charges §2.3) ─────────
            builder.Entity<TypeActe>().HasData(
                new TypeActe
                {
                    Id = 1,
                    Code = "ACT-NAI",
                    Libelle = "Acte de naissance",
                    DelaiCible = 5,
                    PiecesRequises = "Pièce d'identité du déclarant, Livret de famille ou certificat de mariage des parents",
                    EstActif = true
                },
                new TypeActe
                {
                    Id = 2,
                    Code = "ACT-MAR",
                    Libelle = "Acte de mariage",
                    DelaiCible = 7,
                    PiecesRequises = "Pièces d'identité des deux époux, Certificats de célibat, Actes de naissance des époux",
                    EstActif = true
                },
                new TypeActe
                {
                    Id = 3,
                    Code = "ACT-DEC",
                    Libelle = "Acte de décès",
                    DelaiCible = 5,
                    PiecesRequises = "Pièce d'identité du déclarant, Certificat médical de décès",
                    EstActif = true
                }
            );
        }
    }
}
