using ServerShot.Framework.Core.Architecture;

namespace ServerShot.Framework.Core.Plugins.Alerts
{
    public interface ILoggingManager
    {
        void OnLogMessage(LogMessage message, IServerShotModule module = null);
    }
}