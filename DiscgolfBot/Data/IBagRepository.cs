using DiscgolfBot.Data.Models;

namespace DiscgolfBot.Data
{
    public interface IBagRepository
    {
        Task<IEnumerable<BaggedDisc>> GetBaggedDiscs(ulong userId);
        Task<BaggedDiscs> GetBaggedDiscsUpgraded(ulong userId, int multiBagNumber = 0);
        Task<Disc> AddDiscToBag(ulong userId, int discId, int multibagnumber = 0);
        Task<bool> RemoveDiscFromBag(int bagId);
    }
}
