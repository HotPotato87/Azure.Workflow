namespace ServerShot.Framework.Core.Entities.Environment
{
    public class ServerShotEnvironmentBuilder
    {
        public ServerShotEnvironmentBuilder(ServerShotEnvironment serverShotEnvironment)
        {
            Environment = serverShotEnvironment;
        }

        public ServerShotEnvironment Environment { get; private set; }
    }
}