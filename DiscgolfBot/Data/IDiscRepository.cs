using DiscgolfBot.Data.Models;

namespace DiscgolfBot.Data
{
    public interface IDiscRepository
    {
        Task<IEnumerable<Disc>> GetDiscs();
        Task<Disc?> GetDisc(string discName);
    }
}
