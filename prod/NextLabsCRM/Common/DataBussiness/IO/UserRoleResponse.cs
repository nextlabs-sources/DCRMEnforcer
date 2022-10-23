using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public class UserRoleResponse: UserConditionResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ec"></param>
        /// <param name="isPrimaryIDEnforced"> in the request, if the id is enforced already, the isPrimaryIDEnforced is true;
        /// otherwise a xxxid will be added manually, then the isPrimaryIDEnfoced is false.</param>
        public UserRoleResponse(EntityCollection ec,bool isPrimaryIDEnforced):
            base(ec,"roleid",isPrimaryIDEnforced)
        {
        }
    }
}
