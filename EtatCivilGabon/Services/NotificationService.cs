using EtatCivilGabon.Data;
using EtatCivilGabon.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EtatCivilGabon.Services
{
    public class NotificationService : INotificationService
    {
        private readonly MailSettings _settings;
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IOptions<MailSettings> settings,
            AppDbContext context,
            ILogger<NotificationService> logger)
        {
            _settings = settings.Value;
            _context  = context;
            _logger   = logger;
        }

        // ── CONFIRMATION DE DÉPÔT ────────────────────────────────────────────
        public async Task EnvoyerConfirmationDemandeAsync(
            Demande demande, string emailCitoyen)
        {
            var sujet  = $"[État Civil Gabon] Demande reçue — {demande.NumeroSuivi}";
            var corps  = ConstruireCorpsHtml(
                titre:    "Votre demande a été reçue",
                couleur:  "#003DA5",
                icone:    "📋",
                intro:    $"Bonjour {demande.PrenomDemandeur} {demande.NomDemandeur},",
                contenu: $@"
                    <p>Nous avons bien reçu votre demande d'<strong>{demande.TypeActe?.Libelle}</strong>.</p>
                    <p>Votre dossier est en cours d'enregistrement et sera traité dans les meilleurs délais.</p>
                    <div style='background:#f0f4ff;border-left:4px solid #003DA5;padding:12px 16px;margin:16px 0;border-radius:4px;'>
                        <strong>Numéro de suivi :</strong>
                        <span style='font-family:monospace;font-size:1.1em;color:#003DA5;'> {demande.NumeroSuivi}</span>
                    </div>
                    <p>Conservez ce numéro pour suivre l'avancement de votre dossier en ligne.</p>",
                footer: "Délai de traitement estimé : " + (demande.TypeActe?.DelaiCible ?? 0) + " jours ouvrés."
            );

            await EnvoyerEmailAsync(emailCitoyen, sujet, corps, demande.Id,
                $"Confirmation de dépôt de la demande {demande.NumeroSuivi}");
        }

        // ── CHANGEMENT DE STATUT ─────────────────────────────────────────────
        public async Task EnvoyerChangementStatutAsync(
            Demande demande, string emailCitoyen)
        {
            var (libelleStatut, couleur, icone) = demande.Statut switch
            {
                StatutDemande.EnCours => ("En cours de traitement", "#FCD116", "⚙️"),
                StatutDemande.Pret    => ("Prêt à être retiré",     "#009E60", "✅"),
                StatutDemande.Rejete  => ("Rejeté",                 "#dc3545", "❌"),
                _                    => ("Mis à jour",              "#6c757d", "🔔"),
            };

            var sujet = $"[État Civil Gabon] Mise à jour dossier — {demande.NumeroSuivi}";

            var commentaireSection = !string.IsNullOrEmpty(demande.CommentaireAgent)
                ? $@"<div style='background:#fff8e1;border-left:4px solid #FCD116;
                          padding:12px 16px;margin:16px 0;border-radius:4px;'>
                          <strong>Message de l'agent :</strong><br>
                          {demande.CommentaireAgent}
                     </div>"
                : string.Empty;

            var actionSection = demande.Statut == StatutDemande.Pret
                ? @"<div style='background:#f0fff4;border-left:4px solid #009E60;
                        padding:12px 16px;margin:16px 0;border-radius:4px;'>
                        <strong>🎉 Votre document est prêt !</strong><br>
                        Présentez-vous au guichet avec votre numéro de suivi pour le retirer.
                   </div>"
                : string.Empty;

            var corps = ConstruireCorpsHtml(
                titre:   $"Statut de votre dossier : {libelleStatut}",
                couleur: couleur,
                icone:   icone,
                intro:   $"Bonjour {demande.PrenomDemandeur} {demande.NomDemandeur},",
                contenu: $@"
                    <p>Le statut de votre demande d'<strong>{demande.TypeActe?.Libelle}</strong>
                    a été mis à jour.</p>
                    <div style='background:#f0f4ff;border-left:4px solid #003DA5;
                         padding:12px 16px;margin:16px 0;border-radius:4px;'>
                        <strong>Numéro de suivi :</strong>
                        <span style='font-family:monospace;color:#003DA5;'> {demande.NumeroSuivi}</span><br>
                        <strong>Nouveau statut :</strong>
                        <span style='color:{couleur};font-weight:bold;'> {libelleStatut}</span>
                    </div>
                    {commentaireSection}
                    {actionSection}",
                footer: "Vous pouvez suivre votre dossier en ligne avec votre numéro de suivi."
            );

            await EnvoyerEmailAsync(emailCitoyen, sujet, corps, demande.Id,
                $"Changement de statut → {libelleStatut} pour {demande.NumeroSuivi}");
        }

        // ── DOCUMENT PRÊT ────────────────────────────────────────────────────
        public async Task EnvoyerDocumentPretAsync(
            Demande demande, string emailCitoyen)
        {
            var sujet = $"[État Civil Gabon] ✅ Document prêt — {demande.NumeroSuivi}";
            var corps = ConstruireCorpsHtml(
                titre:   "Votre document est prêt !",
                couleur: "#009E60",
                icone:   "✅",
                intro:   $"Bonjour {demande.PrenomDemandeur} {demande.NomDemandeur},",
                contenu: $@"
                    <p>Nous avons le plaisir de vous informer que votre
                    <strong>{demande.TypeActe?.Libelle}</strong> est disponible.</p>
                    <div style='background:#f0fff4;border:2px solid #009E60;
                         padding:16px;margin:16px 0;border-radius:8px;text-align:center;'>
                        <div style='font-size:2em;'>🎉</div>
                        <strong style='color:#009E60;font-size:1.1em;'>Document prêt à être retiré</strong>
                    </div>
                    <div style='background:#f0f4ff;border-left:4px solid #003DA5;
                         padding:12px 16px;margin:16px 0;border-radius:4px;'>
                        <strong>Numéro de suivi :</strong>
                        <span style='font-family:monospace;color:#003DA5;'> {demande.NumeroSuivi}</span>
                    </div>
                    <p><strong>Comment retirer votre document :</strong></p>
                    <ol>
                        <li>Présentez-vous au guichet de votre mairie</li>
                        <li>Munissez-vous d'une pièce d'identité valide</li>
                        <li>Communiquez votre numéro de suivi : <strong>{demande.NumeroSuivi}</strong></li>
                    </ol>",
                footer: "Les horaires d'ouverture sont du lundi au vendredi, de 7h30 à 15h30."
            );

            await EnvoyerEmailAsync(emailCitoyen, sujet, corps, demande.Id,
                $"Document prêt pour {demande.NumeroSuivi}");
        }

        // ── ENVOI SMTP ────────────────────────────────────────────────────────
        private async Task EnvoyerEmailAsync(
            string destinataire,
            string sujet,
            string corpsHtml,
            int demandeId,
            string messageResume)
        {
            var notification = new Notification
            {
                DemandeId         = demandeId,
                Message           = messageResume,
                DateEnvoi         = DateTime.Now,
                EmailDestinataire = destinataire,
                EnvoyeeAvecSucces = false,
            };

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _settings.NomExpediteur,
                    _settings.EmailExpediteur));
                message.To.Add(MailboxAddress.Parse(destinataire));
                message.Subject = sujet;

                var builder = new BodyBuilder { HtmlBody = corpsHtml };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _settings.Host,
                    _settings.Port,
                    _settings.UseSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None);

                await client.AuthenticateAsync(
                    _settings.EmailExpediteur,
                    _settings.MotDePasse);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                notification.EnvoyeeAvecSucces = true;
                _logger.LogInformation(
                    "Email envoyé à {Destinataire} : {Sujet}", destinataire, sujet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Échec de l'envoi de l'email à {Destinataire}", destinataire);
            }
            finally
            {
                // Enregistrer la notification en base dans tous les cas
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
        }

        // ── TEMPLATE HTML ────────────────────────────────────────────────────
        private static string ConstruireCorpsHtml(
            string titre, string couleur, string icone,
            string intro, string contenu, string footer)
        {
            return $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1.0'>
</head>
<body style='margin:0;padding:0;background:#f4f6f9;font-family:Segoe UI,Arial,sans-serif;'>

  <!-- Bandeau tricolore Gabon -->
  <div style='height:5px;background:linear-gradient(to right,#009E60 33.3%,#FCD116 33.3% 66.6%,#003DA5 66.6%);'></div>

  <div style='max-width:600px;margin:24px auto;background:#fff;border-radius:10px;
              box-shadow:0 2px 8px rgba(0,0,0,.08);overflow:hidden;'>

    <!-- En-tête -->
    <div style='background:#1a2c4e;padding:24px 32px;text-align:center;'>
      <div style='font-size:2em;'>{icone}</div>
      <h1 style='color:#fff;margin:8px 0 0;font-size:1.3em;font-weight:700;'>{titre}</h1>
      <p style='color:#adb5bd;margin:4px 0 0;font-size:.85em;'>Plateforme État Civil Gabon</p>
    </div>

    <!-- Corps -->
    <div style='padding:28px 32px;color:#333;font-size:.95em;line-height:1.6;'>
      <p style='margin-top:0;'>{intro}</p>
      {contenu}
    </div>

    <!-- Pied de page -->
    <div style='background:#f8f9fa;border-top:1px solid #e9ecef;
                padding:16px 32px;text-align:center;'>
      <p style='color:#6c757d;font-size:.8em;margin:0;'>{footer}</p>
      <p style='color:#adb5bd;font-size:.75em;margin:8px 0 0;'>
        Cet e-mail a été envoyé automatiquement. Merci de ne pas y répondre.
      </p>
    </div>

    <!-- Bandeau tricolore bas -->
    <div style='height:5px;background:linear-gradient(to right,#009E60 33.3%,#FCD116 33.3% 66.6%,#003DA5 66.6%);'></div>
  </div>

</body>
</html>";
        }
    }
}
