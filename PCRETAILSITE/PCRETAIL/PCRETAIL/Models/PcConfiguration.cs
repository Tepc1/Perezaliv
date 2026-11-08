using System.ComponentModel.DataAnnotations;

namespace PCRETAIL.Models
{
    public class PcConfiguration
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название сборки")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите категорию")]
        public string Category { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = "Pending";

        // В БД хранится только имя файла, полный путь собирается в коде.
        public string? ImagePath { get; set; }

        public ICollection<Component> Components { get; set; } = new List<Component>();
    }
}
