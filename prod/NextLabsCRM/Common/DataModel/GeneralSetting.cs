using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    public enum PolicyDecision
    {
        Deny,
        Allow
    }
    [XmlRoot("generalsetting", Namespace = "urn:nextlabs-crm-schema")]
    public class GeneralSetting
    {
        public static GeneralSetting CreateEmptyInstance(string id)
        {
            //id is useless
            return new GeneralSetting();
        }

        public static void FillDefaultValueIfNotAssigned(ref GeneralSetting setting)
        {
            if(setting == null)
            {
                return;
            }

            if(string.IsNullOrWhiteSpace(setting.SystemErrorMessage))
            {
                setting.SystemErrorMessage = Constant.CacheControl.DefaultExceptionMessage;
            }

            if(string.IsNullOrWhiteSpace(setting.DefaultDenyMessage))
            {
                setting.DefaultDenyMessage = Constant.CacheControl.DefaultDenyMessage;
            }

            if(setting.CacheRefreshInterval <0)
            {
                setting.CacheRefreshInterval = Constant.CacheControl.ValidInterval;
            }
        }
        public GeneralSetting()
        {
            this.PolicyDecision = PolicyDecision.Deny;
            this.SystemErrorMessage = Constant.CacheControl.DefaultExceptionMessage;
            this.DefaultDenyMessage = Constant.CacheControl.DefaultDenyMessage;
            this.CacheRefreshInterval = Constant.CacheControl.ValidInterval;
        }
        [XmlElement("policycontrolhost")]
        public string PolicyControllerHost { get; set; }

        [XmlElement("policycontrolport")]
        public int PolicyControllerPort { get; set; }

        [XmlElement("ishttps")]
        public bool IsHttps { get; set; }

        [XmlElement("oauthserverhost")]
        public string OAuthServerHost { get; set; }

        [XmlElement("oauthport")]
        public int OAuthPort { get; set; }

        [XmlElement("clientid")]
        public string ClientID { get; set; }

        [XmlElement("clientpassword")]
        public string ClientPassword { get; set; }

        [XmlElement("policydecision")]
        public PolicyDecision PolicyDecision { get; set; }

        [XmlElement("message")]
        public string SystemErrorMessage { get; set; }

        [XmlElement("defaultmessage")]
        public string DefaultDenyMessage { get; set; }

        [XmlElement("cacherefreshinterval")]
        public int CacheRefreshInterval{ get;set;}

        [XmlElement("timecostenable")]
        public bool TimeCostEnable { get; set; }

        [XmlElement("spactioneable")]
        public bool SPActionEnable { get; set; }

        [XmlElement("wcusrname")]
        public string wcUsername { get; set; }

        [XmlElement("wcpwd")]
        public string wcPassword { get; set; }

        [XmlElement("userhostip")]
        public string userHostIP { get; set; }
    }
}
