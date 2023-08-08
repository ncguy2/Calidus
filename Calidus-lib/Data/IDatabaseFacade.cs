using System.Linq;

namespace Calidus.lib.Data {
    public class DatabaseFacade<T> where T : class, new(){
        public T createNew() {
            return new T();
        }

        public bool Update(T item) {
            DatabaseService.Instance.Update(item);
            return true;
        }

        public bool Insert(T item) {
            DatabaseService.Instance.Insert(item);
            return true;
        }

        public bool Delete(T item) {
            DatabaseService.Instance.Delete(item);
            return true;
        }

        public IQueryable<T> Query => DatabaseService.Instance.Query(this);
        
        public T? Select(object primaryKey) {
            return DatabaseService.Instance.Select(this, primaryKey);
        }

    }

}