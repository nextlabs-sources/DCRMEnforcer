using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Cache
{
    public delegate T DefaultValue<T>(string key);
    interface ICache<T>
    {
        T Lookup(string key, DefaultValue<T> defaultValue);
        void AddOrUpdate(string key, T value);
        void Clear();
    }
}
