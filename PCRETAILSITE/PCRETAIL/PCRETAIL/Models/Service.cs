using System;
using System.ComponentModel.DataAnnotations;

namespace PCRETAIL.Models
{
    public class Service
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Введите название услуги")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите описание услуги")]
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int ClientId { get; set; }
        public User Client { get; set; } = null!;
        public int? MasterId { get; set; }
        public User? Master { get; set; }
    }
}