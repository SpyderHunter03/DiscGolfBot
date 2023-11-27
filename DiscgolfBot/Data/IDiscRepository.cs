using DiscgolfBot.Data.Models;

namespace DiscgolfBot.Data
{
    public interface IDiscRepository
    {
        Task<IEnumerable<Disc>> GetDiscs();
        Task<Disc?> GetDisc(string discName);
        Task<AdvancedDisc?> GetAdvancedDisc(string discName);
        Task<IEnumerable<DiscPicture>?> GetDiscPictures(string discName);
        Task<IEnumerable<DiscReview>?> GetDiscReviews(string discName);
        Task<IEnumerable<DiscCount>?> GetDiscCounts();
    }
}
