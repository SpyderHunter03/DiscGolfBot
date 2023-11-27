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

        public async Task<AdvancedDisc?> GetAdvancedDisc(string discName)
        {
            var query = $"SELECT discs.*, discstats.maxWeight, discstats.diameter, discstats.height, discstats.rimDepth, discstats.insideRimDiameter, discstats.rimThickness, discstats.approveDate FROM discstats INNER JOIN discs ON discs.id = discstats.discId WHERE LOWER(discs.name) = @DiscName";
            
            using var connection = new MySqlConnection(_connectionString);
            var disc = await connection.QuerySingleOrDefaultAsync<AdvancedDisc>(query, new
            {
                DiscName = discName
            });
            return disc;
        }

        public async Task<IEnumerable<DiscPicture>?> GetDiscPictures(string discName)
        {
            var query = $"SELECT discpic.* FROM discpic INNER JOIN discs ON discs.id = discpic.discId WHERE LOWER(discs.name) = @DiscName";

            using var connection = new MySqlConnection(_connectionString);
            var pictures = await connection.QueryAsync<DiscPicture>(query, new
            {
                DiscName = discName
            });
            return pictures;
        }

        public async Task<IEnumerable<DiscReview>?> GetDiscReviews(string discName)
        {
            var query = $"SELECT discReviews.* FROM discReviews INNER JOIN discs ON discs.id = discReviews.discId WHERE LOWER(discs.name) = @DiscName";

            using var connection = new MySqlConnection(_connectionString);
            var reviews = await connection.QueryAsync<DiscReview>(query, new
            {
                DiscName = discName
            });
            return reviews;
        }

        public async Task<IEnumerable<DiscCount>?> GetDiscCounts()
        {
            var query = $"SELECT discs.NAME as discname, COUNT(*) as count FROM bag INNER JOIN discs ON bag.discid = discs.ID GROUP BY name ORDER BY count DESC";

            using var connection = new MySqlConnection(_connectionString);
            var counts = await connection.QueryAsync<DiscCount>(query);

            return counts;
        }
    }
}
