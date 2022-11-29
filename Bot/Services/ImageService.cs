using Bot.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Bot.Services;

public class ImageService
{
    private readonly IMongoCollection<Image> _imageCollection;

    public ImageService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _imageCollection = mongoDatabase.GetCollection<Image>("images");
    }

    public async Task<List<Image>> GetAsync() =>
        await _imageCollection.Find(_ => true).ToListAsync();

    public async Task<Image?> GetAsync(string id) =>
        await _imageCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<Image> GetRandom(string type)
    {
        var match = new BsonDocument
        {
            { "$match", new BsonDocument { { "type", type } } }                
        };
        var sample = new BsonDocument
        {
            { "$sample", new BsonDocument { { "size", 1 } } }
        };
        var pipeline = new[] { match, sample };  

        return await _imageCollection.AggregateAsync<Image>(pipeline).Result.FirstOrDefaultAsync();
    }
    
    public async Task CreateAsync(Image newImage) =>
        await _imageCollection.InsertOneAsync(newImage);

    public async Task UpdateAsync(string id, Image updatedImage) =>
        await _imageCollection.ReplaceOneAsync(x => x.Id == id, updatedImage);

    public async Task RemoveAsync(string id) =>
        await _imageCollection.DeleteOneAsync(x => x.Id == id); 
}