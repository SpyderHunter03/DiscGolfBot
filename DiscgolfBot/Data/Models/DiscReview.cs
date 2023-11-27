namespace DiscgolfBot.Data.Models
{
    public class DiscReview
    {
        public int Id { get; set; }
        public int DiscId { get; set; }
        public ulong UserId { get; set; }
        public byte[] Review { get; set; } = Array.Empty<byte>();
    }
}
