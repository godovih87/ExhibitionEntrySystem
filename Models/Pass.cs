using System.ComponentModel.DataAnnotations;

namespace ExhibitionEntrySystem.Models
{
    public class Pass
    {
        public int Id { get; set; }

        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; } = null!;

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;

        public int PavilionId { get; set; }
        public Pavilion Pavilion { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }

        [Required]
        public string Status { get; set; } = "Сформирован";

        [Required]
        public string SecretKey { get; set; } = Guid.NewGuid().ToString();

        public ICollection<PassEvent> PassEvents { get; set; } = new List<PassEvent>();
    }
}
