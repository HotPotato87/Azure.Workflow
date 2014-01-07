﻿using System;

namespace Azure.Workflow.Core.Interfaces
{
    public interface IIocContainer
    {
        T Get<T>();
        T Get<T>(Type t);
        void Bind(Type from, Type to);
    }
}