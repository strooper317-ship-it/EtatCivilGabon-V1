namespace EtatCivilGabon.Services
{
    public class MailSettings
    {
        public string Host            { get; set; } = string.Empty;
        public int    Port            { get; set; } = 587;
        public bool   UseSsl          { get; set; } = true;
        public string NomExpediteur   { get; set; } = string.Empty;
        public string EmailExpediteur { get; set; } = string.Empty;
        public string MotDePasse      { get; set; } = string.Empty;
    }
}
