using System;
using Servershot.Framework.Entities.WebJob;

namespace ServerShot.Framework.Core.Plugins
{
    public interface IWebjobSessionPlugin
    {
        void RegisterWebjob(IWebJobBase module);
    }

    public abstract class WebJobSessionPluginBase: IWebjobSessionPlugin
    {
        protected IWebJobBase Module { get; set; }

        public virtual void RegisterWebjob(IWebJobBase module)
        {
            this.Module = module;
        }
    }
}