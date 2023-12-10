using DiscgolfBot.Data.Models;

namespace DiscgolfBot.Data
{
    public interface IDiscRepository
    {
        Task<IEnumerable<DiscDetails>> GetDiscs();
        Task<DiscDetails?> GetDisc(string discName);
        Task<IEnumerable<DiscPicture>?> GetDiscPictures(string discName);
        Task<IEnumerable<DiscReview>?> GetDiscReviews(string discName);
        Task<IEnumerable<DiscCount>?> GetDiscCounts();
        Task<DiscDetails> AddDisc(string discName, int manufacturerId, double speed, double glide, double turn, double fade);
        Task<IEnumerable<Manufacturer>> GetManufacturers();
        Task<Manufacturer?> GetManufacturer(string manufacturerName);
        Task<Manufacturer?> GetManufacturer(int manufacturerId);
        Task<Manufacturer?> AddManufacturer(string manufacturerName);
    }
}
