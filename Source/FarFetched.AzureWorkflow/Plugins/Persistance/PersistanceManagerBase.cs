using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;

namespace ServerShot.Framework.Core.Plugins.Persistance
{
    public interface IPersistanceManager
    {
        string TableName { get; set; }
        Task StoreAsync(string key, object o);
        Task<T> RetrieveAsync<T>(string key);
        Task StoreEnumerableAsync(string table, object o);
        Task<IEnumerable<T>> RetrieveEnumerableAsync<T>(string table);
    }

    public abstract class PersistanceManagerBase : ServerShotSessionPluginBase, IPersistanceManager
    {
        public string TableName { get; set; }

        internal override void OnModuleStarted(IServerShotModule module)
        {
            this.TableName = module.Session.SessionName;
            module.OnStoreAsync += StoreAsync;
            module.OnRetrieveAsync += RetrieveAsync<object>;
            module.OnStoreEnumerableAsync += StoreEnumerableAsync;
            module.OnRetrieveEnumerableAsync += RetrieveEnumerableAsync<object>;
        }

        protected abstract Task OnStoreAsync(string key, object o);

        protected abstract Task<T> OnRetrieveAsync<T>(string key);

        protected abstract Task OnStoreEnumerableAsync(string table, object o);

        protected abstract Task<IEnumerable<T>> OnRetrieveEnumerableAsync<T>(string key);

        public Task StoreAsync(string key, object o)
        {
            return OnStoreAsync(key, o);
        }

        public Task<T> RetrieveAsync<T>(string key)
        {
            return OnRetrieveAsync<T>(key);
        }

        public Task StoreEnumerableAsync(string tablename, object o)
        {
            return OnStoreEnumerableAsync(tablename, o);
        }

        public Task<IEnumerable<T>> RetrieveEnumerableAsync<T>(string tableName)
        {
            return OnRetrieveEnumerableAsync<T>(tableName);
        }
    }
}
