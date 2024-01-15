namespace DiscgolfBot.Data.Models.ViewModels
{
    public class MyDiscs : DiscDetails
    {
        public IList<MyBagDetails>? Discs { get; set; }
    }
}