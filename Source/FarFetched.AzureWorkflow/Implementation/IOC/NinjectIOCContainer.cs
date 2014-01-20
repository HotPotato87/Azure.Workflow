using System;
using Azure.Workflow.Core.Interfaces;
using Ninject;

namespace Azure.Workflow.Core.Implementation.IOC
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