using BookStoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;

namespace BookStoreApi.Services;

public class BooksService
{
    private readonly IMongoCollection<Book> _booksCollection;

    public BooksService()
    {
        // Get Database Connection settings from environment variables
        var mongoHost = Environment.GetEnvironmentVariable("MONGO_HOST");
        var mongoPort = Environment.GetEnvironmentVariable("MONGO_PORT");
        var mongoUser = Environment.GetEnvironmentVariable("MONGO_USER");
        var mongoPass = Environment.GetEnvironmentVariable("MONGO_PASS");
        var mongoDbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME");
        var mongoDbCollection = Environment.GetEnvironmentVariable("MONGO_DB_COLLECTION");

        Console.WriteLine("Environment example from CM:-" + Environment.GetEnvironmentVariable("MONGO_HOST") + "-");
        Console.WriteLine("Environment example from SEC:-" + Environment.GetEnvironmentVariable("MONGO_PASS") + "-");


        var connectionString = $"mongodb://{mongoUser}:{mongoPass}@{mongoHost}:{mongoPort}";
        Console.WriteLine("Connection String:-" + connectionString + "-");

        var mongoClient = new MongoClient(connectionString);

        var mongoDatabase = mongoClient.GetDatabase(mongoDbName);

        _booksCollection = mongoDatabase.GetCollection<Book>(mongoDbCollection);
    }

    public async Task<List<Book>> GetAsync() =>
        await _booksCollection.Find(_ => true).ToListAsync();

    public async Task<Book?> GetAsync(string id) =>
        await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Book newBook) =>
        await _booksCollection.InsertOneAsync(newBook);

    public async Task UpdateAsync(string id, Book updatedBook) =>
        await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _booksCollection.DeleteOneAsync(x => x.Id == id);
}