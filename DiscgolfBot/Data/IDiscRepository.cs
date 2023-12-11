using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;

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
        Task<IEnumerable<DiscPlasticDetails>> GetPlastics();
        Task<IEnumerable<DiscPlasticDetails>> GetPlastics(int manufacturerId);
        Task<IEnumerable<DiscPlasticDetails>> GetPlastics(string manufacturerName);
        Task<DiscPlasticDetails?> GetPlastic(int plasticId);
        Task<IEnumerable<DiscPlasticDetails>?> GetPlastic(string plasticName);
        Task<DiscPlasticDetails?> GetPlastic(string plasticName, string manufacturerName);
        Task<DiscPlasticDetails?> GetPlastic(string plasticName, int manufacturerid);
        Task<DiscPlasticDetails?> AddPlastic(int manufacturerId, string plasticName);


    }
}
