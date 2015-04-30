using System;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Core.Plugins
{
    public interface IJibJobSessionPlugin
    {
        void RegisterWebjob(IJibJobModule module);
    }

    public abstract class WebJobSessionPluginBase: IJibJobSessionPlugin
    {
        protected IJibJobModule Module { get; set; }

        public virtual void RegisterWebjob(IJibJobModule module)
        {
            this.Module = module;
        }
    }
}