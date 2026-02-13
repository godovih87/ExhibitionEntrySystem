namespace ExhibitionEntrySystem.Models
{
    public class PassEvent
    {
        public int Id { get; set; }

        public int PassId { get; set; }
        public Pass Pass { get; set; } = null!;

        public DateTime EventTime { get; set; }
        public string EventType { get; set; } = null!;
    }
}
