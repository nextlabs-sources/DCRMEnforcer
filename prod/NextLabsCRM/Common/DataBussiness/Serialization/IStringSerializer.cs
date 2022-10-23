using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Serialization
{
    public interface IStringSerialize
    {
        object Deserialize(Type type, string source);
        string Serialize(Type type, object obj);
        string SerializeWithoutNamespace(Type type, object obj);
    }
}
