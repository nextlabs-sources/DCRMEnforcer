using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Cache
{
    interface IStringUpdatable
    {
        void Update(string key, string source);
    }
}
