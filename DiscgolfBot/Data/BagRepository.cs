using Dapper;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;
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

        public async Task<IEnumerable<Bag>> GetBags(ulong userId)
        {
            var query = "SELECT * FROM bag WHERE userId = @userId";

            using var connection = new MySqlConnection(_connectionString);
            var baggedDiscs = await connection.QueryAsync<Bag>(query, new { userId });
            return baggedDiscs;
        }

        public async Task<IEnumerable<BaggedDiscs>?> GetBaggedDiscs()
        {
            var query = @"
                SELECT
                    b.*,
                    d.*, m.name AS ManufacturerName,
                    p.*, pm.name AS PutterManufacturerName
                FROM baggeddiscs bd
                INNER JOIN bag b ON b.id = bd.bagId 
                INNER JOIN discs d ON d.id = bd.discId
                INNER JOIN manufacturers m ON m.id = d.manufacturerId
                LEFT JOIN discs p ON p.id = b.putterId
                LEFT JOIN manufacturers pm ON pm.id = p.manufacturerId";

            var bagLookup = new Dictionary<int, BaggedDiscs>();
            using var connection = new MySqlConnection(_connectionString);

            await connection.QueryAsync<BaggedDiscs, DiscDetails, PutterDetails, BaggedDiscs>(
                query,
                (bag, disc, putter) =>
                {
                    if (!bagLookup.TryGetValue(bag.Id, out var baggedDiscs))
                    {
                        baggedDiscs = bag;
                        baggedDiscs.Discs = new List<DiscDetails>();
                        bagLookup.Add(bag.Id, baggedDiscs);
                    }

                    if (bag.PutterId.HasValue && putter != null && baggedDiscs.Putter == null)
                    {
                        putter.ManufacturerName = putter.PutterManufacturerName;
                        baggedDiscs.Putter = putter;
                    }

                    baggedDiscs.Discs.Add(disc);

                    return baggedDiscs;
                },
                splitOn: "Id,Id,Id"
            );

            return bagLookup.Values;
        }

        public async Task<BaggedDiscs?> GetBaggedDiscs(int bagId)
        {
            var query = @"
                SELECT
                    b.*,
                    d.*, m.name AS ManufacturerName,
                    p.*, pm.name AS PutterManufacturerName
                FROM baggeddiscs bd
                INNER JOIN bag b ON b.id = bd.bagId 
                INNER JOIN discs d ON d.id = bd.discId
                INNER JOIN manufacturers m ON m.id = d.manufacturerId
                LEFT JOIN discs p ON p.id = b.putterId
                LEFT JOIN manufacturers pm ON pm.id = p.manufacturerId
                WHERE b.id = @bagId";

            BaggedDiscs? baggedDiscs = null;
            using var connection = new MySqlConnection(_connectionString);

            await connection.QueryAsync<BaggedDiscs, DiscDetails, PutterDetails, BaggedDiscs>(
                query,
                (bag, disc, putter) =>
                {
                    if (baggedDiscs == null || baggedDiscs.Id != bag.Id)
                    {
                        baggedDiscs = bag;
                        baggedDiscs.Discs = new List<DiscDetails>();
                    }

                    if (bag.PutterId.HasValue && putter != null && baggedDiscs.Putter == null)
                    {
                        putter.ManufacturerName = putter.PutterManufacturerName;
                        baggedDiscs.Putter = putter;
                    }

                    baggedDiscs.Discs.Add(disc);

                    return baggedDiscs;
                },
                new { bagId },
                splitOn: "Id,Id,Id"
            );

            return baggedDiscs;
        }

        public async Task<BaggedDiscs?> GetBaggedDiscs(ulong userId, int multiBagNumber = 0)
        {
            var query = @"
                SELECT
                    b.*,
                    d.*, m.name AS ManufacturerName,
                    p.*, pm.name AS PutterManufacturerName
                FROM baggeddiscs bd
                INNER JOIN bag b ON b.id = bd.bagId 
                INNER JOIN discs d ON d.id = bd.discId
                INNER JOIN manufacturers m ON m.id = d.manufacturerId
                LEFT JOIN discs p ON p.id = b.putterId
                LEFT JOIN manufacturers pm ON pm.id = p.manufacturerId
                WHERE b.userId = @userId AND b.multiBagNumber = @multiBagNumber";

            BaggedDiscs? baggedDiscs = null;
            using var connection = new MySqlConnection(_connectionString);

            await connection.QueryAsync<BaggedDiscs, DiscDetails, PutterDetails, BaggedDiscs>(
                query,
                (bag, disc, putter) =>
                {
                    if (baggedDiscs == null || baggedDiscs.Id != bag.Id)
                    {
                        baggedDiscs = bag;
                        baggedDiscs.Discs = new List<DiscDetails>();
                    }

                    if (bag.PutterId.HasValue && putter != null && baggedDiscs.Putter == null)
                    {
                        putter.ManufacturerName = putter.PutterManufacturerName;
                        baggedDiscs.Putter = putter;
                    }

                    baggedDiscs.Discs.Add(disc);

                    return baggedDiscs;
                },
                new { userId, multiBagNumber },
                splitOn: "Id,Id,Id"
            );

            return baggedDiscs;
        }

        private class PutterDetails : DiscDetails
        {
            public string PutterManufacturerName { get; set; }
        }

        public async Task<BaggedDiscs?> GetMyBag(ulong userId, int multiBagNumber = 0)
        {
            var query = @"
                SELECT
                    b.*, 
                    d.*, m.name AS ManufacturerName,
                    p.*, pm.name AS PutterManufacturerName,
                    mb.*, pl.name as Plastic
                FROM baggeddiscs bd
                INNER JOIN bag b ON b.id = bd.bagId
                INNER JOIN discs d ON d.id = bd.discId
                INNER JOIN manufacturers m ON m.id = d.manufacturerId
                LEFT JOIN discs p ON p.id = b.putterId
                LEFT JOIN manufacturers pm ON pm.id = p.manufacturerId
                LEFT JOIN mybag mb ON mb.bagId = bd.bagId AND mb.discId = bd.discId 
                LEFT JOIN plastics pl ON mb.plasticid = pl.id
                WHERE b.userId = @userId AND b.multiBagNumber = @multiBagNumber";

            BaggedDiscs? baggedDiscs = null;
            using var connection = new MySqlConnection(_connectionString);

            await connection.QueryAsync<BaggedDiscs, MyDiscs, PutterDetails, MyBagDetails, BaggedDiscs>(
                query,
                (bag, disc, putter, mybag) =>
                {
                    if (baggedDiscs == null || baggedDiscs.Id != bag.Id)
                    {
                        baggedDiscs = bag;
                        baggedDiscs.Discs = new List<DiscDetails>();
                    }

                    if (bag.PutterId.HasValue && putter != null && baggedDiscs.Putter == null)
                    {
                        putter.ManufacturerName = putter.PutterManufacturerName;
                        baggedDiscs.Putter = putter;
                    }

                    if (!baggedDiscs.Discs.Any(d => d.ManufacturerId == disc.ManufacturerId && d.Name.Equals(disc.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (mybag != null)
                        {
                            disc.Discs ??= new List<MyBagDetails>();
                            disc.Discs.Add(mybag);
                        }

                        baggedDiscs.Discs.Add(disc);
                    } 
                    else if (baggedDiscs.Discs.Any(d => d.ManufacturerId == disc.ManufacturerId && d.Name.Equals(disc.Name, StringComparison.InvariantCultureIgnoreCase)) && mybag != null)
                    {
                        var baggedDisc = baggedDiscs.Discs.First(d => d.ManufacturerId == disc.ManufacturerId && d.Name.Equals(disc.Name, StringComparison.InvariantCultureIgnoreCase));
                        (baggedDisc as MyDiscs)?.Discs?.Add(mybag);
                    }

                    return baggedDiscs;
                },
                new { userId, multiBagNumber },
                splitOn: "Id,Id,Id,Id"
            );

            return baggedDiscs;
        }

        public async Task<Bag> CreateBag(ulong userId)
        {
            var insertQuery = $"INSERT INTO bag (userId, multiBagNumber) VALUES (@userId, 0); SELECT LAST_INSERT_ID();";
            var selectQuery = $"SELECT * FROM bag WHERE id = @id";
            var param = new { userId };

            using var connection = new MySqlConnection(_connectionString);
            var lastInsertedId = await connection.QuerySingleAsync<int>(insertQuery, param);
            var insertedBag = await connection.QuerySingleAsync<Bag>(selectQuery, new { id = lastInsertedId });
            return insertedBag;
        }

        public async Task<Disc> AddDiscToBag(int discId, int bagId)
        {
            var insertQuery = $"INSERT INTO baggeddiscs (discId, bagId) VALUES (@discId, @bagId); SELECT LAST_INSERT_ID();";
            var selectQuery = $"SELECT * FROM discs WHERE id = @id";
            var param = new { discId, bagId };

            using var connection = new MySqlConnection(_connectionString);
            var lastInsertedId = await connection.QuerySingleAsync<int>(insertQuery, param);
            var insertedBaggedDisc = await connection.QuerySingleAsync<Disc>(selectQuery, new { id = lastInsertedId });
            return insertedBaggedDisc;
        }

        public async Task<bool> RemoveDiscFromBag(int discId, int bagId)
        {
            var deleteQuery = $"DELETE FROM baggeddiscs WHERE discid = @discId AND bagid = @bagId";

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { discId, bagId });

            return rowsAffected > 0;
        }

        public async Task<bool> RemoveAllDiscsFromBag(int bagId)
        {
            var deleteQuery = $"DELETE FROM baggeddiscs WHERE bagid = @bagId";

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(deleteQuery, new { bagId });

            return rowsAffected > 0;
        }

        public async Task<Bag> UpdatePutter(int bagId, int putterId)
        {
            var updateQuery = $"UPDATE bag SET putterId = @putterId WHERE id = @bagId";
            var selectQuery = $"SELECT * FROM bag WHERE id = @bagId";
            var param = new { putterId, bagId };

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(updateQuery, param);
            var updatedBag = await connection.QuerySingleAsync<Bag>(selectQuery, param);
            return updatedBag;
        }

        public async Task<MyBag> AddMyBagDisc(int bagId, int discId, int? plasticId, double? weight, string? description, double? speed, double? glide, double? turn, double? fade)
        {
            var insertQuery = $"INSERT INTO mybag (bagId, discId, plasticId, weight, description, speed, glide, turn, fade) VALUES (@bagId, @discId, @plasticId, @weight, @description, @speed, @glide, @turn, @fade); SELECT LAST_INSERT_ID();";
            var selectQuery = $"SELECT * FROM mybag WHERE id = @id";
            var param = new { bagId, discId, plasticId, weight, description, speed, glide, turn, fade };

            using var connection = new MySqlConnection(_connectionString);
            var lastInsertedId = await connection.QuerySingleAsync<int>(insertQuery, param);
            var insertedBaggedDisc = await connection.QuerySingleAsync<MyBag>(selectQuery, new { id = lastInsertedId });
            return insertedBaggedDisc;
        }
    }
}
