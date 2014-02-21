using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShot.Framework.Core.Implementation
{
    public class ServerShotSessionBuilder
    {
        public ServerShotSession ServerShotSession { get; set; }

        public ServerShotSessionBuilder(ServerShotSession session)
        {
            ServerShotSession = session;
        }
    }
}
