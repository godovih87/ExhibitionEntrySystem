namespace ExhibitionEntrySystem.Models
{
    public class ProfileViewModel
    {
        public User User { get; set; } = null!;
        public List<Pass> Passes { get; set; } = new List<Pass>();
    }
}