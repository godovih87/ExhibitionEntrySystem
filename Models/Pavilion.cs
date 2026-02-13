namespace ExhibitionEntrySystem.Models
{
    public class Pavilion
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int MaxSlotsPerHour { get; set; }
    }
}
