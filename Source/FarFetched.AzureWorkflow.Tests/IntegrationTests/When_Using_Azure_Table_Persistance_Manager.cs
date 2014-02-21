using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ServerShot.Framework.Core.Implementation.Persistance;
using Servershot.Framework.Extentions;

namespace ServerShot.Framework.Tests.IntegrationTests
{
    [TestFixture]
    public class When_Using_Azure_Table_Persistance_Manager
    {
        public string _accountName = "serviceshot";
        public string _accountKey = "57Kn2DQl1nX/TjZQyNyNZzDuyfOup0fCfPoT/gZqdYJc8lpygJUq9S6lln3f5O7+s6PzBbArIn2Z+cqMcdqqDg==";
        public string tableName = "people";
        private AzureTablePersistance _persistance;

        [TearDown]
        public void Cleanup()
        {
            var persistance = GetPersistance();

            persistance.DeleteTableAsync(tableName).RunSync();
        }

        [Test]
        public async Task Item_Can_Be_Added_And_Retrived_Via_Enumerable()
        {
            //arrange
            string person1 = "Bob";
            string person2 = "Tom";

            var persistance = GetPersistance();

            //act
            await persistance.StoreEnumerableAsync(tableName, person1);
            await persistance.StoreEnumerableAsync(tableName, person2);

            //assert
            var listOfPerson = await persistance.RetrieveEnumerableAsync<string>(tableName);

            CollectionAssert.AllItemsAreInstancesOfType(listOfPerson, typeof(string));
            CollectionAssert.Contains(listOfPerson, person1);
            CollectionAssert.Contains(listOfPerson, person2);
            Assert.IsTrue(listOfPerson.Count() == 2);
        }

        private AzureTablePersistance GetPersistance()
        {
            return _persistance ?? (_persistance = new AzureTablePersistance(_accountName, _accountKey));
        }
    }
}
