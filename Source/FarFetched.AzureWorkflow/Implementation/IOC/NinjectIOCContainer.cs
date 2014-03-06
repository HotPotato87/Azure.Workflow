using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Parameters;
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
            var result = (T) kernel.Get(t);

            return result;
        }

        public T Get<T>(params object[] parameters)
        {
            return Get<T>(typeof(T),parameters);
        }

        public void Bind<T, T2>()
        {
            this.Bind(typeof(T), typeof(T2));
        }

        public T Get<T>(Type t, params object[] parameters)
        {
            var matchedConstructorParameters = GetConstructorParameters(t, parameters);

            int i = 0;
            var result = (T)kernel.Get(t, parameters.Select(x => (IParameter)new Parameter(matchedConstructorParameters[i++].Name, x, false)).ToArray());

            return result;
        }

        public void Bind(Type from, Type to)
        {
            var binding = kernel.Bind(from).To(to);

            binding.OnActivation(x =>
            {
                if (OnCreate != null)
                {
                    OnCreate(x);
                }
            });
        }

        public void Bind(Type from, Type to, params object[] parameters)
        {
            var matchedConstructorParameters = GetConstructorParameters(to, parameters);

            var binding = kernel.Bind(from).To(to);

            binding.OnActivation(x =>
            {
                if (OnCreate != null)
                {
                    OnCreate(x);
                }
            });

            int i = 0;
            foreach (var paramValue in parameters)
            {
                binding.WithConstructorArgument(matchedConstructorParameters[i].Name, paramValue);
                i++;
            }
        }

        public void BindAsSingleton(Type from, Type to)
        {
            var binding = kernel.Bind(from).To(to).InSingletonScope();

            binding.OnActivation(x =>
            {
                if (OnCreate != null)
                {
                    OnCreate(x);
                }
            });
        }

        public event Action<object> OnCreate;
        

        #region Helpers

        private static ParameterInfo[] GetConstructorParameters(Type t, IEnumerable<object> parameters)
        {
            var matchedConstructor = t.GetConstructor(parameters.Select(x => x.GetType()).ToArray());
            if (matchedConstructor == null)
            {
                throw new WorkflowConfigurationException("constructor for " + t.GetType().FullName + " could not be matched");
            }
            var matchedConstructorParameters = matchedConstructor.GetParameters();
            return matchedConstructorParameters;
        }

        #endregion

    }
}