using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core.Implementation
{
    public class ServerShotSessionBaseBuilder
    {
        public ServerShotSessionBase ServerShotSession { get; set; }

        public ServerShotSessionBaseBuilder(ServerShotSessionBase session)
        {
            ServerShotSession = session;
        }
    }
}
