using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public class UserTeamIDResponse:IRecordIDResponse
    {
        List<Guid> m_teams = new List<Guid>();
        public UserTeamIDResponse(EntityCollection ec)
        {
            if (ec == null)
            {
                throw new ArgumentNullException("entity collection cannot be null");
            }

            foreach (Entity entity in ec.Entities)
            {
                m_teams.Add(entity.GetAttributeValue<Guid>("teamid"));
            }
        }

        public List<Guid> GetRecordIDs()
        {
            return m_teams.Count > 0 ? m_teams : null;
        }
    }
}
