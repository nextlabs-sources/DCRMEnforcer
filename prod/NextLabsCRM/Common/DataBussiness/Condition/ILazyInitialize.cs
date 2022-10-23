using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    interface ILazyInitialize
    {
        bool IsInitialized();
        void Initialize(Guid userId, List<Guid> recordIds);

    }
}
