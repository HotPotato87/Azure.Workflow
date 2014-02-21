using System;
using Ninject;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core.Implementation.IOC
{
    public class NinjectIocContainer : IIocContainer
    {
        private readonly StandardKernel kernel;

        public NinjectIocContainer()
        {
            kernel = new StandardKernel();
        }

        public T Get<T>()
        {
            return kernel.Get<T>();
        }

        public T Get<T>(Type t)
        {
            return (T) kernel.Get(t);
        }

        public void Bind(Type from, Type to)
        {
            kernel.Bind(from).To(to);
        }
    }
}