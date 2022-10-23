using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Serialization
{
    public class FakeStingSerialzer : IStringSerialize
    {
        object IStringSerialize.Deserialize(Type type, string source)
        {
            return null;
        }

        string IStringSerialize.Serialize(Type type, object obj)
        {
            return null;
        }

        string IStringSerialize.SerializeWithoutNamespace(Type type, object obj)
        {
            return null;
        }
    }
}
