using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Implementation.IOC;
using ServerShot.Framework.Core.Interfaces;
using Servershot.Framework.Entities;

namespace ServerShot.Framework.Core.Entities.Environment
{
    public class ServerShotEnvironment
    {
        public IIocContainer IOCContainer { get; set; }

        public ServerShotEnvironment()
        {
            IOCContainer = new NinjectIocContainer();
        }

        public static ServerShotEnvironmentBuilder BuildEnvironment()
        {
            return new ServerShotEnvironmentBuilder(new ServerShotEnvironment());
        }

        public ServerShotSessionBase CreateContinousSession()
        {
            return new ServerShotContinuousSession()
            {
                Environment = this
            };
        }

        public ServerShotLinearSession CreateLinearSession()
        {
            return new ServerShotLinearSession()
            {
                Environment = this
            };
        }
    }
}