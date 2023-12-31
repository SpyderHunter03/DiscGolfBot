﻿using Dapper;
using DiscgolfBot.Data.Models;
using DiscgolfBot.Data.Models.ViewModels;
using MySql.Data.MySqlClient;
using System;

namespace DiscgolfBot.Data
{
    public class DiscRepository : IDiscRepository
    {
        private readonly string _connectionString;

        public DiscRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DiscDetails>> GetDiscs()
        {
            var query = "SELECT d.*, m.name manufacturerName FROM discs d INNER JOIN manufacturers m ON m.id = d.manufacturerId";

            using var connection = new MySqlConnection(_connectionString);
            var discs = await connection.QueryAsync<DiscDetails>(query);
            return discs;
        }

        public async Task<DiscDetails?> GetDisc(string discName)
        {
            var query = "SELECT d.*, m.name manufacturerName FROM discs d INNER JOIN manufacturers m ON m.id = d.manufacturerId WHERE LOWER(d.name) = @discName";

            using var connection = new MySqlConnection(_connectionString);
            var disc = await connection.QuerySingleOrDefaultAsync<DiscDetails>(query, new { discName = discName.ToLower() });
            return disc;
        }

        public async Task<DiscDetails?> GetDisc(int discId)
        {
            var query = "SELECT d.*, m.name manufacturerName FROM discs d INNER JOIN manufacturers m ON m.id = d.manufacturerId WHERE d.id = @discId";

            using var connection = new MySqlConnection(_connectionString);
            var disc = await connection.QuerySingleOrDefaultAsync<DiscDetails>(query, new { discId });
            return disc;
        }

        public async Task<IEnumerable<DiscPicture>?> GetDiscPictures(string discName)
        {
            var query = $"SELECT discpic.* FROM discpic INNER JOIN discs ON discs.id = discpic.discId WHERE LOWER(discs.name) = @discName";

            using var connection = new MySqlConnection(_connectionString);
            var pictures = await connection.QueryAsync<DiscPicture>(query, new { discName = discName.ToLower() });
            return pictures;
        }

        public async Task<IEnumerable<DiscReview>?> GetDiscReviews(string discName)
        {
            var query = $"SELECT discreviews.* FROM discreviews INNER JOIN discs ON discs.id = discreviews.discId WHERE LOWER(discs.name) = @discName";

            using var connection = new MySqlConnection(_connectionString);
            var reviews = await connection.QueryAsync<DiscReview>(query, new { discName = discName.ToLower() });
            return reviews;
        }

        public async Task<IEnumerable<DiscCount>?> GetDiscCounts()
        {
            var query = $"SELECT d.name as discname, COUNT(*) as count FROM baggeddiscs bd INNER JOIN discs d ON d.id = bd.discId GROUP BY name ORDER BY count DESC";

            using var connection = new MySqlConnection(_connectionString);
            var counts = await connection.QueryAsync<DiscCount>(query);

            return counts;
        }

        public async Task<DiscDetails?> AddDisc(string discName, int manufacturerId, double speed, double glide, double turn, double fade)
        {
            var insertQuery = $@"
                INSERT INTO discs (name, manufacturerId, speed, glide, turn, fade) 
                VALUES (@discName, @manufacturerId, @speed, @glide, @turn, @fade)";

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(insertQuery, new
                { discName, manufacturerId, speed, glide, turn, fade });
            return await GetDisc(discName);
        }

        public async Task<IEnumerable<Manufacturer>> GetManufacturers()
        {
            var query = "SELECT * FROM manufacturers";

            using var connection = new MySqlConnection(_connectionString);
            var manufacturers = await connection.QueryAsync<Manufacturer>(query);
            return manufacturers;
        }

        public async Task<Manufacturer?> GetManufacturer(string manufacturerName)
        {
            var query = $"SELECT * FROM manufacturers WHERE LOWER(name) = @manufacturerName";

            using var connection = new MySqlConnection(_connectionString);
            var manufacturer = await connection.QuerySingleOrDefaultAsync<Manufacturer>(query, new
            { manufacturerName = manufacturerName.ToLower() });
            return manufacturer;
        }

        public async Task<Manufacturer?> GetManufacturer(int manufacturerId)
        {
            var query = $"SELECT * FROM manufacturers WHERE id = @manufacturerId";

            using var connection = new MySqlConnection(_connectionString);
            var manufacturer = await connection.QuerySingleOrDefaultAsync<Manufacturer>(query, new { manufacturerId });
            return manufacturer;
        }

        public async Task<Manufacturer?> AddManufacturer(string manufacturerName)
        {
            var insertQuery = $"INSERT INTO manufacturers (name) VALUES (@manufacturerName)";

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(insertQuery, new { manufacturerName });
            return await GetManufacturer(manufacturerName);
        }

        public async Task<IEnumerable<DiscPlasticDetails>> GetPlastics()
        {
            var query = "SELECT p.*, m.name AS manufacturerName FROM plastics p INNER JOIN manufacturers m ON m.id = p.manufacturerId";

            using var connection = new MySqlConnection(_connectionString);
            var plastics = await connection.QueryAsync<DiscPlasticDetails>(query);
            return plastics;
        }

        public async Task<IEnumerable<DiscPlasticDetails>> GetPlastics(int manufacturerId)
        {
            var query = "SELECT p.*, m.name AS manufacturerName FROM plastics p INNER JOIN manufacturers m ON m.id = p.manufacturerId WHERE p.manufacturerId = @manufacturerId";

            using var connection = new MySqlConnection(_connectionString);
            var plastics = await connection.QueryAsync<DiscPlasticDetails>(query, new { manufacturerId });
            return plastics;
        }

        public async Task<IEnumerable<DiscPlasticDetails>> GetPlastics(string manufacturerName)
        {
            var query = "SELECT p.*, m.name AS manufacturerName FROM plastics p INNER JOIN manufacturers m ON m.id = p.manufacturerId WHERE m.name = @manufacturerName";

            using var connection = new MySqlConnection(_connectionString);
            var plastics = await connection.QueryAsync<DiscPlasticDetails>(query, new { manufacturerName });
            return plastics;
        }

        public async Task<DiscPlasticDetails?> GetPlastic(int plasticId)
        {
            var query = "SELECT p.*, m.name AS manufacturerName FROM plastics p INNER JOIN manufacturers m ON m.id = p.manufacturerId WHERE id = @plasticId";

            using var connection = new MySqlConnection(_connectionString);
            var plastic = await connection.QuerySingleOrDefaultAsync<DiscPlasticDetails>(query, new { plasticId });
            return plastic;
        }

        public async Task<IEnumerable<DiscPlasticDetails>?> GetPlastic(string plasticName)
        {
            var query = "SELECT p.*, m.name AS manufacturerName FROM plastics p INNER JOIN manufacturers m ON m.id = p.manufacturerId WHERE LOWER(p.name) = @plasticName";

            using var connection = new MySqlConnection(_connectionString);
            var plastic = await connection.QueryAsync<DiscPlasticDetails>(query, new { plasticName = plasticName.ToLower() });
            return plastic;
        }

        public async Task<DiscPlasticDetails?> GetPlastic(string plasticName, string manufacturerName)
        {
            var query = "SELECT p.*, m.name AS manufacturerName FROM plastics p INNER JOIN manufacturers m ON m.id = p.manufacturerId WHERE LOWER(p.name) = @plasticName AND LOWER(m.name) = @manufacturerName";

            using var connection = new MySqlConnection(_connectionString);
            var plastic = await connection.QuerySingleOrDefaultAsync<DiscPlasticDetails>(query, new { plasticName = plasticName.ToLower(), manufacturerName = manufacturerName.ToLower() });
            return plastic;
        }

        public async Task<DiscPlasticDetails?> GetPlastic(string plasticName, int manufacturerid)
        {
            var query = "SELECT p.*, m.name AS manufacturerName FROM plastics p INNER JOIN manufacturers m ON m.id = p.manufacturerId WHERE LOWER(p.name) = @plasticName AND p.manufacturerId = @manufacturerId";

            using var connection = new MySqlConnection(_connectionString);
            var plastic = await connection.QuerySingleOrDefaultAsync<DiscPlasticDetails>(query, new { plasticName = plasticName.ToLower(), manufacturerid });
            return plastic;
        }

        public async Task<DiscPlasticDetails?> AddPlastic(int manufacturerId, string plasticName)
        {
            var insertQuery = $"INSERT INTO plastics (manufacturerId, name) VALUES (@manufacturerId, @plasticName)";

            using var connection = new MySqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteAsync(insertQuery, new { manufacturerId, plasticName });
            return await GetPlastic(plasticName, manufacturerId);
        }
    }
}
