using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common
{
    class NXLIDProvider
    {
        private static byte[] m_cNXLSuffix = {0x4E,0x58,0x4C,0x49,0x44}; //NXLID
        public static Guid CreateNewNXLGUID()
        {
            byte[] byGuid = Guid.NewGuid().ToByteArray();
            int nGuidLen = byGuid.Length;
            for (int index = 0; index < m_cNXLSuffix.Length; index++)
            {
                byte bySuffix = m_cNXLSuffix[index];
                int nGuidByteOffset = nGuidLen - 1 - index;
                byGuid[nGuidByteOffset] = bySuffix;
                //byGuid[index] = bySuffix;
            }

            return new Guid(byGuid);
        }

       public static bool IsNXLGUID(Guid guid)
        {
            if (guid == null)
            {
                return false;
            }

            byte[] byGuid = guid.ToByteArray();
            int nGuidLen = byGuid.Length;
            for (int index = 0; index < m_cNXLSuffix.Length; index++)
            {
                byte bySuffix = m_cNXLSuffix[index];
                int nGuidByteOffset = nGuidLen - 1 - index;
                if(byGuid[nGuidByteOffset] != bySuffix)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
