using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Cache
{
    
    public class MemoryCache<T> : ICache<T>
    {
        
        private ConcurrentDictionary<string, T> m_dic = new ConcurrentDictionary<string, T>();
        
        public virtual T Lookup(string key, DefaultValue<T> defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            T value;

            if(m_dic.TryGetValue(key, out value))
            {
                return value;
            }

            if (defaultValue == null)
            {
                return default(T);
            }

            value = defaultValue(key);

            if(value != null)
            {
                m_dic.AddOrUpdate(key, value, (k, v) => value);
            }

            return value;
        }

        public virtual List<string> GetKeys()
        {
            return m_dic.Keys.ToList<string>();
        }

        public virtual List<T> GetValues()
        {
            return m_dic.Values.ToList();
        }
        public void AddOrUpdate(string key, T value)
        {
            if (key == null)
            {
                return;
            }

            if (value == null)
            {
                return;
            }
            m_dic.AddOrUpdate(key, value, (k, v) => value);
        }

        public void Clear()
        {
            m_dic.Clear();
        }
    }
}
