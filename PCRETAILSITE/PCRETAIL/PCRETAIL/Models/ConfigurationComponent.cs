namespace PCRETAIL.Models
{
    // Этот класс больше не используется:
    // для связи конфигураций и компонентов используется стандартная
    // many‑to‑many навигация через коллекции в PcConfiguration и Component.
    public class ConfigurationComponent
    {
        public int ConfigurationId { get; set; }
        public PcConfiguration Configuration { get; set; } = null!;

        public int ComponentId { get; set; }
        public Component Component { get; set; } = null!;
    }
}
