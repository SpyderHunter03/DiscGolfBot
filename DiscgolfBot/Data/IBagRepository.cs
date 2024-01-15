using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;

namespace DiscgolfBot.Data
{
    public interface IBagRepository
    {
        Task<IEnumerable<Bag>> GetBags(ulong userId);
        Task<IEnumerable<BaggedDiscs>?> GetBaggedDiscs();
        Task<BaggedDiscs?> GetBaggedDiscs(int bagId);
        Task<BaggedDiscs> GetBaggedDiscs(ulong userId, int multiBagNumber = 0);
        Task<bool> RemoveAllDiscsFromBag(int bagId);
        Task<Bag> CreateBag(ulong userId);
        Task<Disc> AddDiscToBag(int discId, int bagId);
        Task<bool> RemoveDiscFromBag(int discId, int bagId);
        Task<Bag> UpdatePutter(int bagId, int putterId);
        Task<MyBag> AddMyBagDisc(int bagId, int discId, int? plasticId, double? weight, string? description, double? speed, double? glide, double? turn, double? fade);
        Task<BaggedDiscs?> GetMyBag(ulong userId, int multiBagNumber = 0);
    }
}
