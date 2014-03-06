using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Servershot.Framework.Entities;

namespace ServerShot.Framework.Tests.UnitTests.Session
{
    public class ContinuousSessionTests
    {
        [Test]
        public void ContinuousSession_Defaults_To_Never_Fail_True()
        {
            var session = new ServerShotContinuousSession();

            //assert
            Assert.IsTrue(session.Settings.NeverFail);
        }
    }
}
