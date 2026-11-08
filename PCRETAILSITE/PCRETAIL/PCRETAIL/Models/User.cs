using Microsoft.Extensions.Configuration;
using PCRETAIL.Models;
using System.Data;

namespace PCRETAIL.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public ICollection<PcConfiguration> PcConfigurations { get; set; } = new List<PcConfiguration>();
        public ICollection<Service> ClientServices { get; set; } = new List<Service>();
        public ICollection<Service> MasterServices { get; set; } = new List<Service>();
    }
}
