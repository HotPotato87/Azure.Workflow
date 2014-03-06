using ServerShot.Framework.Core.Entities.Environment;

namespace ServerShot.Framework.Core.Helpers
{
    public class EnvironmentHelpers
    {
        public static ServerShotEnvironment BuildStandardEnvironment()
        {
            return new ServerShotEnvironment();
        }
    }
}