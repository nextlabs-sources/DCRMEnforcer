using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public interface IRecordResponse: IRecordIDResponse
    {
        List<Entity> GetRecords();
    }
}
