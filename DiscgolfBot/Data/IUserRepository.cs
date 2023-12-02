using DiscgolfBot.Data.Models;

namespace DiscgolfBot.Data
{
    public interface IUserRepository
    {
        Task<IEnumerable<AdminUser>> GetAdminUsers();
        Task<bool> IsAdminUser(ulong userId);
    }
}
