using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Architecture;

namespace ServerShot.Framework.Core.Plugins.Persistance
{
    public interface IPersistanceManager : IServerShotSessionBasePlugin
    {
        Task StoreAsync(string key, object o);
        Task<T> RetrieveAsync<T>(string key);
        Task StoreEnumerableAsync(string table, object o);
        Task<IEnumerable<T>> RetrieveEnumerableAsync<T>(string table);
    }

    public abstract class PersistanceManagerBase : ServerShotSessionBasePluginBase, IPersistanceManager
    {
        public override void OnModuleStarted(IServerShotModule module)
        {
            this.Module = module;
            module.OnStoreAsync += StoreAsync;
            module.OnRetrieveAsync += RetrieveAsync<object>;
            module.OnStoreEnumerableAsync += StoreEnumerableAsync;
            module.OnRetrieveEnumerableAsync += RetrieveEnumerableAsync<object>;
        }

        protected IServerShotModule Module { get; set; }

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
