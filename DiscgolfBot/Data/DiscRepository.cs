using Dapper;
using DiscgolfBot.Data.Models;
using MySql.Data.MySqlClient;

namespace DiscgolfBot.Data
{
    public class DiscRepository : IDiscRepository
    {
        private readonly string _connectionString;

        public DiscRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Disc>> GetDiscs()
        {
            var query = "SELECT * FROM discs";

            using var connection = new MySqlConnection(_connectionString);
            var discs = await connection.QueryAsync<Disc>(query);
            return discs;
        }

        public async Task<Disc?> GetDisc(string discName)
        {
            var query = $"SELECT * FROM discs WHERE LOWER(name) = @DiscName";

            using var connection = new MySqlConnection(_connectionString);
            var disc = await connection.QuerySingleOrDefaultAsync<Disc>(query, new
            {
                DiscName = discName
            });
            return disc;
        }
    }
}
