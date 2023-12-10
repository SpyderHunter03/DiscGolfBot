namespace DiscgolfBot.Data.Models
{
    public class AceTracker
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public int DiscId { get; set; }
        public int PlasticId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Distance { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Pot { get; set; }
    }
}
