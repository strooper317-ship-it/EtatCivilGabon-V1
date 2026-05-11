using Microsoft.EntityFrameworkCore.Migrations;

// ============================================================
// MIGRATION INITIALE — EtatCivilGabon
// ============================================================
// Cette migration est générée automatiquement par EF Core.
// Pour la créer dans Visual Studio, exécuter dans la console :
//
//   dotnet ef migrations add InitialCreate
//   dotnet ef database update
//
// Ou dans le Package Manager Console :
//   Add-Migration InitialCreate
//   Update-Database
// ============================================================

#nullable disable

namespace EtatCivilGabon.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Table TypesActes
            migrationBuilder.CreateTable(
                name: "TypesActes",
                columns: table => new
                {
                    Id             = table.Column<int>(nullable: false)
                                         .Annotation("SqlServer:Identity", "1, 1"),
                    Code           = table.Column<string>(maxLength: 20, nullable: false),
                    Libelle        = table.Column<string>(maxLength: 100, nullable: false),
                    DelaiCible     = table.Column<int>(nullable: false),
                    PiecesRequises = table.Column<string>(maxLength: 500, nullable: false),
                    EstActif       = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesActes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TypesActes_Code",
                table: "TypesActes",
                column: "Code",
                unique: true);

            // Table Demandes
            migrationBuilder.CreateTable(
                name: "Demandes",
                columns: table => new
                {
                    Id               = table.Column<int>(nullable: false)
                                           .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroSuivi      = table.Column<string>(maxLength: 20, nullable: false),
                    TypeActeId       = table.Column<int>(nullable: false),
                    CitoyenId        = table.Column<string>(nullable: false),
                    Statut           = table.Column<string>(nullable: false, defaultValue: "EnAttente"),
                    DateSoumission   = table.Column<DateTime>(nullable: false),
                    DateMiseAJour    = table.Column<DateTime>(nullable: true),
                    CommentaireAgent = table.Column<string>(maxLength: 1000, nullable: true),
                    NomDemandeur     = table.Column<string>(maxLength: 100, nullable: false),
                    PrenomDemandeur  = table.Column<string>(maxLength: 100, nullable: false),
                    ObjetDemande     = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demandes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Demandes_TypesActes_TypeActeId",
                        column: x => x.TypeActeId,
                        principalTable: "TypesActes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_NumeroSuivi",
                table: "Demandes",
                column: "NumeroSuivi",
                unique: true);

            // Table PiecesJointes
            migrationBuilder.CreateTable(
                name: "PiecesJointes",
                columns: table => new
                {
                    Id           = table.Column<int>(nullable: false)
                                        .Annotation("SqlServer:Identity", "1, 1"),
                    DemandeId    = table.Column<int>(nullable: false),
                    NomFichier   = table.Column<string>(maxLength: 255, nullable: false),
                    CheminFichier= table.Column<string>(maxLength: 500, nullable: false),
                    TypeMime     = table.Column<string>(maxLength: 100, nullable: false),
                    Taille       = table.Column<long>(nullable: false),
                    DateDepot    = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiecesJointes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PiecesJointes_Demandes_DemandeId",
                        column: x => x.DemandeId,
                        principalTable: "Demandes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Table Notifications
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id                  = table.Column<int>(nullable: false)
                                              .Annotation("SqlServer:Identity", "1, 1"),
                    DemandeId           = table.Column<int>(nullable: false),
                    Message             = table.Column<string>(maxLength: 500, nullable: false),
                    DateEnvoi           = table.Column<DateTime>(nullable: false),
                    EnvoyeeAvecSucces   = table.Column<bool>(nullable: false),
                    EmailDestinataire   = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Demandes_DemandeId",
                        column: x => x.DemandeId,
                        principalTable: "Demandes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Seed des 3 types d'actes (cahier des charges §2.3)
            migrationBuilder.InsertData(
                table: "TypesActes",
                columns: new[] { "Id", "Code", "Libelle", "DelaiCible", "PiecesRequises", "EstActif" },
                values: new object[,]
                {
                    { 1, "ACT-NAI", "Acte de naissance", 5, "Pièce d'identité du déclarant, Livret de famille ou certificat de mariage des parents", true },
                    { 2, "ACT-MAR", "Acte de mariage",   7, "Pièces d'identité des deux époux, Certificats de célibat, Actes de naissance des époux", true },
                    { 3, "ACT-DEC", "Acte de décès",     5, "Pièce d'identité du déclarant, Certificat médical de décès", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Notifications");
            migrationBuilder.DropTable(name: "PiecesJointes");
            migrationBuilder.DropTable(name: "Demandes");
            migrationBuilder.DropTable(name: "TypesActes");
        }
    }
}
