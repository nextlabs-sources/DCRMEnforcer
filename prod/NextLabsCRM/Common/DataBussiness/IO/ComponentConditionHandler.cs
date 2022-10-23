using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    class ComponentConditionHandler
    {
        private UserConditionRequest m_request;
        public UserConditionRequest Request { get; }
        public UserConditionResponse Response { get; set; }
        public ComponentConditionHandler(UserConditionRequest request)
        {
            if(request == null)
            {
                throw new ArgumentNullException("request cannot be null.");
            }
            m_request = request;
        }
    }
}
