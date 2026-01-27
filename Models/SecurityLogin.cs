namespace ExhibitionEntrySystem.Models
{
    public class SecurityLogin
    {
        public int Id { get; set; }
        public string LoginType { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
