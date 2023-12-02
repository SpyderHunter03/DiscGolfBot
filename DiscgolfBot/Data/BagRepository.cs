using Dapper;
using DiscgolfBot.Data.Models;
using MySql.Data.MySqlClient;

namespace DiscgolfBot.Data
{
    public class BagRepository : IBagRepository
    {
        private readonly string _connectionString;

        public BagRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<BaggedDisc>> GetBaggedDiscs(ulong userId)
        {
            var query = "SELECT * FROM bag WHERE userId = @userId";

            using var connection = new MySqlConnection(_connectionString);
            var baggedDiscs = await connection.QueryAsync<BaggedDisc>(query, new { userId });
            return baggedDiscs;
        }

        public async Task<Disc> AddDiscToBag(ulong userId, int discId, int multibagnumber = 0)
        {
            var insertQuery = $"INSERT INTO bag (userId, discId, multibagnumber) VALUES (@userId, @discId, @multibagnumber)";
            var selectQuery = $"SELECT * FROM bag WHERE userId = @userId AND discId = @discId";
            var param = new { userId, discId, multibagnumber };

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(insertQuery, param);
            var insertedBaggedDisc = await connection.QuerySingleAsync<Disc>(selectQuery, param);
            return insertedBaggedDisc;
        }

        public async Task<bool> RemoveDiscFromBag(int bagId)
        {
            var deleteQuery = $"DELETE FROM bag WHERE id = @bagId";

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { bagId });

            return rowsAffected > 0;
        }
    }
}
