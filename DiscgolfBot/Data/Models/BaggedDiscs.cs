namespace DiscgolfBot.Data.Models
{
    public class BaggedDiscs
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public int MultiBagNumber { get; set; }
        public string? BagName { get; set; }
        public string? BagPhoto { get; set; }
        public int? PutterId { get; set; }
        public DiscDetails? Putter { get; set; }
        //public string? PutterName { get; set; }
        public IList<DiscDetails> Discs { get; set; }
    }
}
