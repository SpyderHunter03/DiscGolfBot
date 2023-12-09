using Dapper;
using DiscgolfBot.Data.Models;
using MySql.Data.MySqlClient;
using System.Linq;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

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

        public async Task<BaggedDiscs?> GetBaggedDiscsUpgraded(ulong userId, int multiBagNumber = 0)
        {
            var query = @"
                SELECT 
                    b.*,
                    (SELECT discs.name FROM discs WHERE discs.id = b.putterId) putterName,
                    d.* 
                FROM bag b
                INNER JOIN discs d ON b.DiscId = d.Id
                WHERE b.userId = @userId AND b.multiBagNumber = @multiBagNumber";

            var lookup = new Dictionary<int, BaggedDiscs>();
            using var connection = new MySqlConnection(_connectionString);

            var result = await connection.QueryAsync<BaggedDiscs, Disc, BaggedDiscs>(
                query,
                (b, d) =>
                {
                    if (!lookup.TryGetValue(b.MultiBagNumber, out var baggedDisc))
                        lookup.Add(b.MultiBagNumber, baggedDisc = b);

                    baggedDisc.Discs ??= new List<Disc>();
                    baggedDisc.Discs.Add(d); /* Add discs to bag */

                    return baggedDisc;
                },
                new { userId, multiBagNumber }
                //splitOn: "Id" // Adjust this to the name of the first column of the second object (Disc in this case)
            );

            lookup.TryGetValue(multiBagNumber, out var retval);
            return retval;
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
