using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public class UserConditionResponse : IRecordResponse
    {
        List<Guid> m_primaryIDs = new List<Guid>();
        List<Entity> m_entites = new List<Entity>();
        public UserConditionResponse(EntityCollection ec, string primaryIDName, bool isPrimaryIDEnforced)
        {
            if (ec == null)
            {
                throw new ArgumentNullException("entity collection cannot be null");
            }

            if(string.IsNullOrWhiteSpace(primaryIDName))
            {
                throw new ArgumentNullException("Primary id name cannot be null");
            }

            foreach (Entity entity in ec.Entities)
            {
                if (!entity.Id.Equals(Guid.Empty))
                {
                    m_primaryIDs.Add(entity.Id);
                }

                if (!isPrimaryIDEnforced)
                {
                    entity.Attributes.Remove(primaryIDName);
                }
            }
            m_entites.AddRange(ec.Entities);
        }

        public List<Guid> GetRecordIDs()
        {
            return m_primaryIDs.Count > 0 ? m_primaryIDs : null;
        }

        public List<Entity> GetRecords()
        {
            return m_entites.Count > 0 ? m_entites : null;
        }
    }
}
