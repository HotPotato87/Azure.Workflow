using NUnit.Framework;
using ServerShot.Framework.Core.Entities.Environment;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Tests.UnitTests.Webjob
{
    public class WebJobPluginsTests
    {
        [Test]
        public void EmailSummaryPlugin()
        {
            var session = CreateSession();


        }

        private WebJobSession CreateSession()
        {
            return new WebJobSession(new ServerShotEnvironment());
        }
    }
}