namespace DiscgolfBot.Data.Models
{
    public class MyBag
    {
        public int Id { get; set; }
        public int BagId { get; set; }
        public int DiscId { get; set; }
        public int PlasticId { get; set; }
        public decimal? Weight { get; set; }
        public string? Description { get; set; }
        public decimal? Speed { get; set; }
        public decimal? Glide { get; set; }
        public decimal? Turn { get; set; }
        public decimal? Fade { get; set; }

        public string FlightNumbers(string? separator = "/") =>
            $"{Speed:0.#}{separator}{Glide:0.#}{separator}{Turn:0.#}{separator}{Fade:0.#}";
    }
}
