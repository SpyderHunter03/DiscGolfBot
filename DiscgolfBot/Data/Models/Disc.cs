namespace DiscgolfBot.Data.Models
{
    public class Disc
    {
        public int Id { get; set; }
        public int ManufacturerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Speed { get; set; }
        public decimal Glide { get; set; }
        public decimal Turn { get; set; }
        public decimal Fade { get; set; }
        public decimal? MaxWeight { get; set; }
        public decimal? Diameter { get; set; }
        public decimal? Height { get; set; }
        public decimal? RimDepth { get; set; }
        public decimal? InsideRimDiameter { get; set; }
        public decimal? RimThickness { get; set; }
        public DateTime? ApproveDate { get; set; }

        public string FlightNumbers(string? separator = " ") =>
            $"{Speed:0.#}{separator}{Glide:0.#}{separator}{Turn:0.#}{separator}{Fade:0.#}";
    }
}
