using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
namespace NextLabs.CRMEnforcer.Common.DataBussiness.Cache
{
    class RegularCache<T> : MemoryCache<T>, IStringUpdatable
    {
        private IStringSerialize m_serializer = new FakeStingSerialzer();

        public RegularCache(IStringSerialize serializer)
        {
            if (serializer != null)
            {
                this.m_serializer = serializer;
            }
        }

    protected virtual void BeforeUpdate(string key,T bizValue)
        {
            //DO NOTHING
        }
    public  virtual void  Update(string key, string xmlValue)
        {
            if (key == null)
            {
                //log
                return;
            }

            if (string.IsNullOrWhiteSpace(xmlValue))
            {
                //log
                return;
            }

            T bizValue = (T)m_serializer.Deserialize(typeof(T), xmlValue);
            if(bizValue != null)
            {
                BeforeUpdate(key, bizValue);
                AddOrUpdate(key, bizValue);
            }
        }
    }
}
