using System.Linq.Expressions;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace PersonalFinanceManager.Infrastructure.Data.Context;

public class LiteDbContext : IDisposable
{
    private ILiteDatabase Database { get; set; }
    public LiteDbContext(IConfiguration config)
    {
        var connectionString = config
            .GetValue<string>("LiteDb:ConnectionString");
        
        Database = new LiteDatabase(connectionString);
    }

    private ILiteCollection<T> GetCollection<T>() where T : class
    {
        return Database.GetCollection<T>(typeof(T).Name);
    }

    public bool Save<T>(T entity) where T : class
    {
        var collection = GetCollection<T>();
        return collection.Upsert(entity); // true if insert
    }

    public T? GetItemById<T>(string entityId) where T : class
    {
        return GetCollection<T>().FindById(entityId);
    }

    public T? GetItemByQuery<T>(Expression<Func<T, bool>>? predicate, bool asc=true, string orderBy="") where T : class
    {
        return GetItemsByQuery(predicate, 0, 1, asc, orderBy)
            .FirstOrDefault();
    }
    
    public IEnumerable<T> GetItemsByQuery<T>(Expression<Func<T, bool>>? predicate, int skip=0, int limit=100, bool asc=true, string orderBy="") where T : class
    {
        var col = GetCollection<T>().Query();
        
        if(predicate != null)
            col = col.Where(predicate);
        
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            col = asc 
                ? col.OrderBy(orderBy)
                : col.OrderByDescending(orderBy);
        }
        
        return col
            .Skip(skip)
            .Limit(limit)
            .ToList();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Database.Dispose();
    }
}