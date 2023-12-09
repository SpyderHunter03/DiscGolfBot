﻿namespace DiscgolfBot.Data.Models
{
    public class BaggedDisc
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public int DiscId { get; set; }
        public int MultiBagNumber { get; set; }
        public string? BagName { get; set; }
        public string? BagPhoto { get; set; }
        public int? PutterId { get; set; }
    }
}
