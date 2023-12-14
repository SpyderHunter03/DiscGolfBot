﻿using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;

namespace DiscgolfBot.Data
{
    public interface IBagRepository
    {
        Task<IEnumerable<Bag>> GetBags(ulong userId);
        Task<BaggedDiscs> GetBaggedDiscs(ulong userId, int multiBagNumber = 0);
        Task<Bag> CreateBag(ulong userId);
        Task<Disc> AddDiscToBag(int discId, int bagId);
        Task<bool> RemoveDiscFromBag(int discId, int bagId);
        Task<Bag> UpdatePutter(int bagId, int putterId);
    }
}
