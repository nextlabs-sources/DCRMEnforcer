using Microsoft.Xrm.Sdk;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle
{
    public class DisplayAlertMessageHandle
    {
        private string m_strMessage = null;
        private string m_strDisplayMessage = null;
        private const string Split = "\r\n";
        public DisplayAlertMessageHandle(List<CEObligation> lisObs,string strMessage)
        {

            List<DisplayAlertMessageModel> lisObligation = DisplayAlertMessageModel.GetCurrentObligation(lisObs);
            StringBuilder sbAlertMessage = new StringBuilder();
            if (lisObligation != null)
            {

                foreach (DisplayAlertMessageModel item in lisObligation)
                {
                    sbAlertMessage.Append(item.Message + Split);
                }
            }
            m_strDisplayMessage = sbAlertMessage.ToString();
            m_strMessage = strMessage;
        }
        public DisplayAlertMessageHandle(string strDefaultMessage, string strMessage)
        {
            m_strDisplayMessage = strDefaultMessage;
            m_strMessage = strMessage;
        }
        public void DoObligation()
        {
            switch(m_strMessage)
            {
                case Common.Constant.MessageName.Retrievemultiple:
                    {
                        throw new InvalidPluginExecutionException(m_strDisplayMessage.ToString());
                    }
                case Common.Constant.MessageName.Update:
                    {
                        throw new InvalidPluginExecutionException(m_strDisplayMessage.ToString());
                    }
                    break;
                default:
                    {
                        throw new Exceptions.InvalidMessageException("DisplayAlertMessageHandle", "DoObligation", m_strMessage);
                    }
                    break;
            }
        }
        public string GetDisplayMessage()
        {
            return m_strDisplayMessage;
        }
    }
}
