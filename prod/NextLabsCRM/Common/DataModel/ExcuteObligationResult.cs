using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace NextLabs.CRMEnforcer.Common.DataModel.Share
{
    [DataContract]
   public  class ExcuteObligationResult
    {
        public ExcuteObligationResult(string strDecision)
        {
            Decision = strDecision;
            MaskFields = new List<MaskField>();
            DisplayMessages = new List<string>();
        }
        [DataMember]
        public string Decision { get; set; }

        [DataMember]
        public List<MaskField> MaskFields { get; set; }

        [DataMember]
        public List<string> DisplayMessages { get; set; }

        public override string ToString()
        {
            return JavaPC.RestAPISDK.JsonSerializer.SaveToJson(this);
        }

        public static ExcuteObligationResult LoadFromJson(string strJson)
        {
            return JavaPC.RestAPISDK.JsonSerializer.LoadFromJson<ExcuteObligationResult>(strJson);
        }
    }
    [DataContract]
    public class MaskField
    {
        public MaskField(string strMaskCharacter)
        {
            MaskCharacter = strMaskCharacter;
            Fields = new List<string>();
        }
        [DataMember]
        public string MaskCharacter { get; set; }
        [DataMember]
        public List<string> Fields { get; set; }
    }
}
