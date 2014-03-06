using System;
using System.Security.Policy;

namespace ServerShot.Framework.Core.Interfaces
{
    public interface IIocContainer
    {
        T Get<T>();
        T Get<T>(Type t);
        T Get<T>(params object[] parameters);
        void Bind<T, T2>();
        void Bind(Type from, Type to);
        void Bind(Type from, Type to, params object[] parameters);
        event Action<object> OnCreate;
        void BindAsSingleton(Type from, Type to);
    }
}