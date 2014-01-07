using System;
using Azure.Workflow.Core.Interfaces;
using Ninject;

namespace Azure.Workflow.Core.Implementation.IOC
{
    public class NinjectIOCContainer : IIocContainer
    {
        private readonly StandardKernel kernel;

        public NinjectIOCContainer()
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