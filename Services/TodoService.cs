using MongoDB.Driver;
using TodoApi.Models;

namespace TodoApi.Services
{
    public class TodoService
    {
        private readonly IMongoCollection<TodoItem> _todoItems;

        public TodoService(IConfiguration config)
        {
            var settings = config.GetSection("TodoDatabaseSettings").Get<TodoDatabaseSettings>();
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _todoItems = database.GetCollection<TodoItem>(settings.CollectionName);
        }

        public async Task<List<TodoItem>> GetAllAsync() =>
            await _todoItems.Find(_ => true).ToListAsync();

        public async Task<TodoItem?> GetAsync(string id) =>
            await _todoItems.Find(t => t.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(TodoItem todoItem) =>
            await _todoItems.InsertOneAsync(todoItem);

        public async Task UpdateAsync(string id, TodoItem updatedItem) =>
            await _todoItems.ReplaceOneAsync(t => t.Id == id, updatedItem);

        public async Task DeleteAsync(string id) =>
            await _todoItems.DeleteOneAsync(t => t.Id == id);
    }
}
