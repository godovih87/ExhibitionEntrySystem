using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ExhibitionEntrySystem.Models
{
    public class Booking
    {
        [Required(ErrorMessage = "Организация обязательна")]
        public string OrganizationName { get; set; } = null!;

        [Required(ErrorMessage = "Контактное лицо обязательно")]
        public string ContactPerson { get; set; } = null!;

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Выберите тип транспорта")]
        public string VehicleType { get; set; } = null!;

        [Required(ErrorMessage = "Гос. номер обязателен")]
        [RegularExpression(@"^[а-яА-Я]\d{3}[а-яА-Я]{2}\d{2,3}$",
            ErrorMessage = "Неверный формат номера (пример: а111аа96)")]
        public string LicensePlate { get; set; } = null!;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
