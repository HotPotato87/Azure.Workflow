using System;
using System.Collections.Generic;
using System.Linq;
using ServerShot.Framework.Core.Architecture;
using ServerShot.Framework.Core.Interfaces;
using Moq;

namespace ServerShot.Framework.Tests.Helpers
{
    public static class TestHelpers
    {
        public static Mock<ICloudQueueFactory> CreateNonEmptyStubQueueFactory()
        {
            var mockFactory = new Mock<ICloudQueueFactory>();
            mockFactory.Setup(x => x.CreateQueue(It.IsAny<IServerShotModule>())).Returns(() => CreateNonEmptyCloudQueue().Object);
            return mockFactory;
        }

        public static Mock<ICloudQueue> CreateNonEmptyCloudQueue()
        {
            List<object> data = new[]
            {
                new object(),
                new object()
            }.ToList();

            var mockCloudQueue = new Mock<ICloudQueue>();
            Func<List<object>> getData = () =>
            {
                List<object> result = data.ToList();
                data.Clear();
                return result;
            };

            mockCloudQueue.Setup(x => x.ReceieveAsync<object>(It.IsAny<int>())).Returns(async () => getData());

            return mockCloudQueue;
        }
    }
}