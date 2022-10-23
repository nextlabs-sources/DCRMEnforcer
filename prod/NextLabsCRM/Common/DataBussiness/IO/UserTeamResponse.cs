using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public class UserTeamResponse: UserConditionResponse
    {
        List<Guid> m_teams = new List<Guid>();
        public UserTeamResponse(EntityCollection ec, bool isPrimaryIDEnforced):
            base(ec,"teamid",isPrimaryIDEnforced)
        {
        }
    }
}
