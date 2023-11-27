namespace DiscgolfBot.Data.Models
{
    public class DiscPicture
    {
        public int Id { get; set; }
        public int DiscId { get; set; }
        public ulong UserId { get; set; }
        public string Link { get; set; } = string.Empty;
    }
}
