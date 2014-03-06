using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShot.Framework.Core.Queue;
using NUnit.Framework;

namespace ServerShot.Framework.Tests.UnitTests
{
    [TestFixture]
    public class InMemoryQueueTests
    {
        [Test]
        public async Task Adding_To_Queue_Results_In_Message_Added_To_List()
        {
            //arrange
            var listToAdd = new List<object>() { new object() };
            var inMemoryQueue = new InMemoryQueue();
            
            //act
            inMemoryQueue.AddToAsync(listToAdd);

            //assert
            Assert.IsTrue(inMemoryQueue._inMemoryCollection.Count == 1);
        }

        [Test]
        public async Task Recieving_Batch_Of_Messages_From_Queue_Returns_From_List()
        {
            //arrange
            var obj = new object();
            var listToAdd = new List<object>() { obj };
            var inMemoryQueue = new InMemoryQueue();
            listToAdd.ForEach(x=>inMemoryQueue._inMemoryCollection.Enqueue(x));

            //act
            var result = await inMemoryQueue.ReceieveAsync<object>(1);

            //assert
            Assert.IsTrue(result.ToList()[0] == obj);
        }
    }
}
