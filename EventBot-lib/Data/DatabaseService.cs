#nullable enable
using System;
using System.Linq;
using System.Reflection;

namespace EventBot.lib.Data {
    public class DatabaseService {
        private static DatabaseService? _instance;

        public delegate void OnInsert(Type type, object data);

        public event OnInsert OnInsertEvent = null!;

        public void RegisterOnInsertEvent<T>(Action<T> func) {
            OnInsertEvent += (type, data) => {
                if (type == typeof(T) && data is T castData)
                    func(castData);
            };
        }

        public static DatabaseService Get() {
            return _instance ??= new DatabaseService();
        }

        public static DatabaseService Instance => Get();
        private IDatabaseDriver driver = null!;

        private DatabaseService() {}

        public string GetCreateString<T>() {
            DBColumnAttribute[] fields = GetFields<T>();
            return driver.GetCreateString(GetTableName<T>(), fields);
        }

        public void CreateTable<T>() {
            DBColumnAttribute[] fields = GetFields<T>();
            driver.CreateTable(GetTableName<T>(), fields);
        }

        public void Update<T>(T item) where T : new() {
            driver.Update(item);
        }

        public void Insert<T>(T item) where T : new() {
            driver.Insert(item);
            OnInsertEvent?.Invoke(typeof(T), item!);
        }

        public void Delete<T>(T item) where T : new() {
            driver.Delete(item);
        }

        public IQueryable<T> Query<T>(DatabaseFacade<T> databaseFacade) where T : class, new() {
            return driver.Query(databaseFacade);
        }
        
        public T? Select<T>(DatabaseFacade<T> databaseFacade, object primaryKey) where T : class, new() {
            return driver.Select(databaseFacade, primaryKey);
        }

        public static string GetTableName<T>() => typeof(T).Name;
        
        public static DBColumnAttribute[] GetFields<T>() =>
            typeof(T).GetFields()
                     .Select(x => x.GetCustomAttribute<DBColumnAttribute>()?.setField(x))
                     .Where(x => x != null)
                     .ToArray()!;

        public void SetDriver(IDatabaseDriver driver) {
            this.driver = driver;
        }

    }
}