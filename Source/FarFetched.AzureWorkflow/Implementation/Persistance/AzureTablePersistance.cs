using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Azure.Workflow.Core.Plugins.Persistance;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Azure.Workflow.Core.Implementation.Persistance
{
    public class AzureTablePersistance : PersistanceManagerBase
    {
        private readonly string _accountName;
        private readonly string _accountKey;
        private readonly string _sessionName;
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        private bool _isInitialised = false;

        public AzureTablePersistance(string accountName, string accountKey)
        {
            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey))
            {
                throw new InvalidCredentialException("You must provide an account name and an account key");
            }

            _accountName = accountName;
            _accountKey = accountKey;
        }

        public override string Validate(WorkflowSession module)
        {
            if (string.IsNullOrEmpty(module.SessionName))
            {
                return "Workflow session must specify a name for Azure table persistance to be used. See WorkflowSession.SessionName";
            }

            return base.Validate(module);
        }

        public override async Task OnStoreAsync(string key, object o)
        {
            if (!this._isInitialised) await Initialize();

            var entity1 = new DynamicTableEntity();
            entity1.PartitionKey = "generic";
            entity1.RowKey = key;
            entity1[key] = EntityProperty.CreateEntityPropertyFromObject(o);

            TableOperation operation = TableOperation.InsertOrReplace(entity1);
            _table.Execute(operation);
        }

        public override async Task<object> OnRetrieveAsync(string key)
        {
            if (!this._isInitialised) await Initialize();

            var prop = _table.CreateQuery<DynamicTableEntity>();
            var result = _table.ExecuteQuery(prop).FirstOrDefault();
            return result[key].PropertyAsObject;
        }

        private async Task Initialize()
        {
            _storageAccount = CloudStorageAccount.Parse(String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", _accountName, _accountKey));

            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();

            _table = tableClient.GetTableReference(this.TableName);

            if (!await _table.ExistsAsync())
            {
                await _table.CreateAsync();
            }

            this._isInitialised = true;
        }
    }
}
