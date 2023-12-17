using DiscgolfBot.Data.Models;

namespace DiscgolfBot.Data
{
    public interface IAdminRepository
    {
        Task<IEnumerable<AdminUser>> GetAdmins();
    }
}