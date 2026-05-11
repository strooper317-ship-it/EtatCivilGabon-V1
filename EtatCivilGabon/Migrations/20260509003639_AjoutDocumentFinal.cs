using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtatCivilGabon.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDocumentFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheminDocumentFinal",
                table: "Demandes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEnvoiDocument",
                table: "Demandes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomDocumentFinal",
                table: "Demandes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TailleDocumentFinal",
                table: "Demandes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeMimeDocumentFinal",
                table: "Demandes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheminDocumentFinal",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "DateEnvoiDocument",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "NomDocumentFinal",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "TailleDocumentFinal",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "TypeMimeDocumentFinal",
                table: "Demandes");
        }
    }
}
