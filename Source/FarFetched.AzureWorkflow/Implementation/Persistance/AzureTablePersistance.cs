using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ServerShot.Framework.Core.Plugins.Persistance;

namespace ServerShot.Framework.Core.Implementation.Persistance
{
    public class AzureTablePersistance : PersistanceManagerBase
    {
        private readonly string _accountName;
        private readonly string _accountKey;
        private readonly string _sessionName;
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private bool _isInitialised = false;
        private CloudTableClient _tableClient;

        public AzureTablePersistance(string accountName, string accountKey)
        {
            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey))
            {
                throw new InvalidCredentialException("You must provide an account name and an account key");
            }

            _accountName = accountName;
            _accountKey = accountKey;
        }

        public AzureTablePersistance(string accountName, string accountKey, string tableName) : this(accountName, accountKey)
        {
            this.TableName = tableName;
        }

        public override string Validate(ServerShotSession module)
        {
            if (string.IsNullOrEmpty(module.SessionName))
            {
                return "ServerShot session must specify a name for Azure table persistance to be used. See ServerShotSession.SessionName";
            }

            return base.Validate(module);
        }

        protected override async Task OnStoreAsync(string key, object o)
        {
            if (!this._isInitialised) await Initialize();

            var entity1 = new DynamicTableEntity();
            entity1.PartitionKey = "generic";
            entity1.RowKey = key;
            entity1[key] = EntityProperty.CreateEntityPropertyFromObject(o);

            TableOperation operation = TableOperation.InsertOrReplace(entity1);
            _table.Execute(operation);
        }

        protected override async Task<T> OnRetrieveAsync<T>(string key)
        {
            if (!this._isInitialised) await Initialize();

            var prop = _table.CreateQuery<DynamicTableEntity>();
            var result = _table.ExecuteQuery(prop).FirstOrDefault();
            return (T)result[key].PropertyAsObject;
        }

        protected override async Task OnStoreEnumerableAsync(string table, object o)
        {
            this.TableName = table;
            if (!this._isInitialised) await Initialize();

            var entity1 = new DynamicTableEntity();
            entity1.PartitionKey = table;
            entity1.RowKey = DateTime.Now.Ticks.ToString();
            entity1["item"] = EntityProperty.CreateEntityPropertyFromObject(o);

            TableOperation operation = TableOperation.InsertOrReplace(entity1);
            _table.Execute(operation);
        }

        protected override async Task<IEnumerable<T>> OnRetrieveEnumerableAsync<T>(string table)
        {
            this.TableName = table;
            if (!this._isInitialised) await Initialize();

            var prop = _table.CreateQuery<DynamicTableEntity>();
            var result = _table.ExecuteQuery(prop).ToList();
            return result.Select(x=>x["item"].PropertyAsObject).Cast<T>();
        }

        private async Task Initialize(string tableName = "")
        {
            if (string.IsNullOrEmpty(this.TableName))
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    throw new Exception("TableName has not been set");
                }
                else
                {
                    this.TableName = tableName;
                }
            }

            _storageAccount = CloudStorageAccount.Parse(String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", _accountName, _accountKey));

            _tableClient = _storageAccount.CreateCloudTableClient();

            _table = _tableClient.GetTableReference(this.TableName);

            if (!await _table.ExistsAsync())
            {
                await _table.CreateAsync();
            }

            this._isInitialised = true;
        }

        public async Task DeleteTableAsync(string tableName)
        {
            if (!this._isInitialised) await Initialize(tableName);

            await _table.DeleteAsync();
        }
    }
}
