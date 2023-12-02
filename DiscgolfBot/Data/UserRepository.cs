using Dapper;
using DiscgolfBot.Data.Models;
using MySql.Data.MySqlClient;

namespace DiscgolfBot.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<AdminUser>> GetAdminUsers()
        {
            var query = "SELECT * FROM adminusers";

            using var connection = new MySqlConnection(_connectionString);
            var admins = await connection.QueryAsync<AdminUser>(query);
            return admins;
        }

        public async Task<bool> IsAdminUser(ulong userId)
        {
            var query = "SELECT * FROM adminusers WHERE userid = @userId";

            using var connection = new MySqlConnection(_connectionString);
            var admin = await connection.QuerySingleOrDefaultAsync<AdminUser>(query, new { userId });
            return admin != null;
        }
    }
}
