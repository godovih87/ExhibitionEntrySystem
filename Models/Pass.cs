using System.ComponentModel.DataAnnotations;

namespace ExhibitionEntrySystem.Models
{
    public class Pass
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

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

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<PassEvent> PassEvents { get; set; } = new List<PassEvent>();
    }
}