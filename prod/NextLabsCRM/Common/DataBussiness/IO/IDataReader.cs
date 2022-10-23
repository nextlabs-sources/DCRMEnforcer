using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataModel;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    interface IDataReader<T>
    {
        T Read(string key);
        List<T> Read(List<string> keys);
        List<T> ReadAll();
    }
}
