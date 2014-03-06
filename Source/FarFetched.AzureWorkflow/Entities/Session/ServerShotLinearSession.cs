using ServerShot.Framework.Core.Implementation;

namespace Servershot.Framework.Entities
{
    public class ServerShotLinearSession : ServerShotSessionBase
    {
        public static ServerShotSessionBaseBuilder StartBuild()
        {
            return new ServerShotSessionBaseBuilder(new ServerShotLinearSession());
        }
    }
}