using System.Linq;
using Discord;

namespace EventBot.lib.Data {
    public interface IDatabaseDriver {
        void Close();
        
        void CreateTable(string tableName, params DBColumnAttribute[] fields);

        void Insert<T>(T item);
        void Update<T>(T item);
        void Delete<T>(T item);

        string GetCreateString(string tableName, DBColumnAttribute[] fields);

        IQueryable<T> Query<T>(DatabaseFacade<T> facade) where T : class, new();
        T? Select<T>(DatabaseFacade<T> databaseFacade, object primaryKey) where T : class, new();
    }
}