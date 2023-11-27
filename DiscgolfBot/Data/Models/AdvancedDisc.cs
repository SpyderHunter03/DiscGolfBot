namespace DiscgolfBot.Data.Models
{
    public class AdvancedDisc : Disc
    {
        public double MaxWeight { get; set; }
        public double Diameter { get; set; }
        public double Height { get; set; }
        public double RimDepth { get; set; }
        public double InsideRimDiameter { get; set; }
        public double RimThickness { get; set; }
        public DateTime ApproveDate { get; set; }
    }
}
