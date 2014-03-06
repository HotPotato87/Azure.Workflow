using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using ServerShot.Framework.Core.Interfaces;

namespace ServerShot.Framework.Core
{
    public static class IOC
    {
        public static IIocContainer Kernel { get; set; }
    }
}
