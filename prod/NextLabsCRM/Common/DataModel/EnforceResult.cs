using NextLabs.JavaPC.RestAPISDK.CEModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace NextLabs.CRMEnforcer.Common.DataModel
{
    public class EnforceResult
    {
        public CEResponse Decision { get; set; }
        public List<CEObligation> ListObligations { get; set; }

        public override string ToString()
        {
            ShareParam.EnforcerResult result = new ShareParam.EnforcerResult();
            result.Decision = this.Decision.ToString();

            result.Obligations = new List<ShareParam.Obligation>();
            
            foreach(CEObligation item in this.ListObligations)
            {
                ShareParam.Obligation ob = new ShareParam.Obligation();
                ob.Name = item.Get_Nmae();
                CEAttres ceAttr = item.GetCEAttres();
                ob.Attrs = new List<ShareParam.Attr>();
                for(int i=0;i<ceAttr.get_count();i++)
                {
                    ShareParam.Attr attr = new ShareParam.Attr();
                    string strKey, strValue = string.Empty;
                    CEAttributeType ceType= CEAttributeType.XACML_AnyURI;
                    ceAttr.Get_Attre(i,out strKey, out strValue, out ceType);
                    if(!string.IsNullOrEmpty(strKey)&&!string.IsNullOrEmpty(strValue)&&!strKey.Equals(strValue))
                    {
                        attr.key = strKey;
                        attr.Value = strValue;
                        attr.Type = ceType.ToString();
                        ob.Attrs.Add(attr);
                    }
                }
                result.Obligations.Add(ob);
            }
            return JavaPC.RestAPISDK.JsonSerializer.SaveToJson(result);
        }

        public static EnforceResult LoadFromJson(string strJson)
        {
            ShareParam.EnforcerResult getedEnforcerResult = (ShareParam.EnforcerResult)JavaPC.RestAPISDK.JsonSerializer.LoadFromJson<ShareParam.EnforcerResult>(strJson);
            EnforceResult result = null;
            if (getedEnforcerResult!=null)
            {
                result = new EnforceResult();
                result.Decision =(CEResponse)Enum.Parse(typeof(CEResponse), getedEnforcerResult.Decision);
                result.ListObligations = new List<CEObligation>();
                foreach (ShareParam.Obligation item in getedEnforcerResult.Obligations)
                {
                    CEAttres ceAttrs = new CEAttres();
                    foreach(ShareParam.Attr attr in item.Attrs)
                    {
                        CEAttribute ceAttr = new CEAttribute(attr.key, attr.Value,(CEAttributeType)Enum.Parse(typeof(CEAttributeType),attr.Type));
                        ceAttrs.Add_Attre(ceAttr);
                    }

                    result.ListObligations.Add(new CEObligation(item.Name, ceAttrs, string.Empty));
                }
            }
            return result;

        }
    }
}
