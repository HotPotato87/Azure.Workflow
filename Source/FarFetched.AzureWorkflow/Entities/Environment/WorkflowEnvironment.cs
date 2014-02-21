using ServerShot.Framework.Core.Implementation;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Entities.Environment
{
    public class WorkflowEnvironment
    {
        public IIocContainer IOCContainer { get; set; }

        public static WorkflowEnvironmentBuilder BuildEnvironment()
        {
            return new WorkflowEnvironmentBuilder(new WorkflowEnvironment());
        }

        public ServerShotSession CreateSession()
        {
            return new ServerShotSession(this);
        }
    }
}