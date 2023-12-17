using Dapper;
using DiscgolfBot.Data.Models;
using MySql.Data.MySqlClient;

namespace DiscgolfBot.Data
{
    public class AdminRepository(string connectionString) : IAdminRepository
    {
        private readonly string _connectionString = connectionString;

        public async Task<IEnumerable<AdminUser>> GetAdmins()
        {
            var query = "SELECT * FROM adminusers";

            using var connection = new MySqlConnection(_connectionString);
            var admins = await connection.QueryAsync<AdminUser>(query);
            return admins;
        }
    }
}
