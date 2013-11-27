using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace FarFetched.AzureWorkflow.Core
{
    public static class IOC
    {
        private static readonly StandardKernel _kernel;

        public static T Get<T>()
        {
            return _kernel.Get<T>();
        }

        static IOC()
        {
            _kernel = new StandardKernel();
        }
    }
}
