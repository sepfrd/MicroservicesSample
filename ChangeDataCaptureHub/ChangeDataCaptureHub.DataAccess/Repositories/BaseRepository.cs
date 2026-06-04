using ChangeDataCaptureHub.Model.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ChangeDataCaptureHub.DataAccess.Repositories;

public class BaseRepository<T> : IBaseRepository<T>
    where T : BaseDocument
{
    private readonly IMongoCollection<T> _mongoDbCollection;

    protected BaseRepository(IOptions<MongoDbSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

        _mongoDbCollection = mongoDatabase.GetCollection<T>(databaseSettings.Value.CollectionName);
    }

    public async Task CreateOneAsync(T entity, CancellationToken cancellationToken = default) =>
        await _mongoDbCollection.InsertOneAsync(entity, cancellationToken: cancellationToken);

    public async Task CreateManyAsync(IEnumerable<T> values, CancellationToken cancellationToken = default) =>
        await _mongoDbCollection.InsertManyAsync(values, cancellationToken: cancellationToken);

    public async Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var deleteResult = await _mongoDbCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);

        return deleteResult.DeletedCount == 1;
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _mongoDbCollection.Find(_ => true).ToListAsync(cancellationToken);

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filterDefinition = Builders<T>.Filter.Eq(x => x.Id, id);

        var documentCursor = await _mongoDbCollection.FindAsync<T>(filterDefinition, cancellationToken: cancellationToken);

        return await documentCursor.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateOneAsync(T entity, CancellationToken cancellationToken = default)
    {
        var filterDefinition = Builders<T>.Filter.Eq(x => x.Id, entity.Id);

        var resultCursor = await _mongoDbCollection.ReplaceOneAsync(filterDefinition, entity, cancellationToken: cancellationToken);

        return resultCursor.ModifiedCount == 1;
    }
}