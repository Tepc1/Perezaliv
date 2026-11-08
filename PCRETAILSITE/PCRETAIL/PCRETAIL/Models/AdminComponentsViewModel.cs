using System.ComponentModel.DataAnnotations;

namespace PCRETAIL.Models
{
    public class AdminComponentsViewModel
    {
        [Required(ErrorMessage = "Введите название комплектующего")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите тип комплектующего")]
        public string Type { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "Цена не может быть отрицательной")]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        public List<Component> Components { get; set; } = new();
    }
}
