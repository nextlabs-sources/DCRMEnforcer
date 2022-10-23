using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    public class ObligationModelBase
    {
        protected static Regex regexObligationIndex = new Regex(@"^\S*[^0-9]([0-9]+)$");
        protected const char Obligation_Split_Char = ':';

        protected static bool GetObligationAttributeRealName(string strGetdAttrName, out string strRealAttrName, out string strFlag)
        {
            bool bResult = false;
            strRealAttrName = string.Empty;
            strFlag = string.Empty;
            Match matchObligationIndex = regexObligationIndex.Match(strGetdAttrName);
            if (matchObligationIndex.Groups != null && matchObligationIndex.Groups.Count > 1)
            {
                strFlag = matchObligationIndex.Groups[matchObligationIndex.Groups.Count - 1].Value;
                if (strGetdAttrName.Length > strFlag.Length)
                {
                    strRealAttrName = strGetdAttrName.Substring(0, strGetdAttrName.Length - strFlag.Length);
                    bResult = true;
                }
            }
            return bResult;
        }

        

    }
    public class ApplySecurityFilterModel : ObligationModelBase
    {
        private const string Obligation_Name_ApplySecurityFilter = "app_sec_filter";
        private const string Obligation_Attr_NamePfx_Column = "col";
        private const string Obligation_Attr_NamePfx_Operate = "op";
        private const string Obligation_Attr_NamePfx_Value = "val";

        public string Field { get; set; }
        public ConditionOperator Operator { get; set; }
        public string Value { get; set; }
        public Type ValueType { get; set; }
        public static List<Dictionary<string, ApplySecurityFilterModel>> GetCurrentObligation(List<CEObligation> lisObligations,MemoryCache<SecureEntity> secureEntityCache,string strEntityName)
        {
            List<Dictionary<string, ApplySecurityFilterModel>> lisResult = new List<Dictionary<string, ApplySecurityFilterModel>>();
            foreach (CEObligation item in lisObligations)
            {
                if (item.Get_Nmae().Equals(Obligation_Name_ApplySecurityFilter))
                {
                    Dictionary<string, ApplySecurityFilterModel> obGeted = TransfromFromCEObligation(item, secureEntityCache, strEntityName);
                    if (obGeted.Count > 0)
                    {
                        lisResult.Add(obGeted);
                    }
                }
            }
            return lisResult;
        }

        public static Dictionary<string, ApplySecurityFilterModel> TransfromFromCEObligation(CEObligation ob, MemoryCache<SecureEntity> secureEntityCache, string strCurrentEntityName)
        {
            Dictionary<string, ApplySecurityFilterModel> dirResult = new Dictionary<string, ApplySecurityFilterModel>();
            CEAttres ceAttrs = ob.GetCEAttres();
            int nCeArrtLength = ceAttrs.get_count();
            string strKey = string.Empty;
            string strValue = string.Empty;
            CEAttributeType ceType;
            for (int nIndex = 0; nIndex < nCeArrtLength; nIndex++)
            {
                ceAttrs.Get_Attre(nIndex, out strKey, out strValue, out ceType);
                if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strValue)&& !strKey.Equals(strValue))
                {
                    string strRealName = string.Empty;
                    string strFlag = string.Empty;
                    if (GetObligationAttributeRealName(strKey, out strRealName, out strFlag))
                    {
                        if (!dirResult.ContainsKey(strFlag))
                        {
                            dirResult.Add(strFlag, new ApplySecurityFilterModel());
                        }
                        switch (strRealName.ToLower())
                        {
                            case Obligation_Attr_NamePfx_Column:
                                {
                                    dirResult[strFlag].Field = strValue;
                                }
                                break;
                            case Obligation_Attr_NamePfx_Operate:
                                {
                                    ConditionOperator conditionOperatorObligationGeted;
                                    if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.EqualsTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.Equal;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.NotEqualTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.NotEqual;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.GreaterThan, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.GreaterThan;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.LessThan, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.LessThan;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.GreaterThanOrEqualsTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.GreaterEqual;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.LessThanOrEqualsTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.LessEqual;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.IsNULL, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.Null;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.IsNotNULL, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.NotNull;
                                    }
                                    else if(strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.OlderThanXdays, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.OlderThanXDays;
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidObligationException(ob);
                                    }
                                    dirResult[strFlag].Operator = conditionOperatorObligationGeted;
                                }
                                break;
                            case Obligation_Attr_NamePfx_Value:
                                {
                                    dirResult[strFlag].Value = strValue;
                                }
                                break;
                        }
                    }
                }
            }
            List<string> lisInvaildObligationItem = new List<string>();
            foreach(KeyValuePair<string, ApplySecurityFilterModel> item in dirResult)
            {
                if(string.IsNullOrEmpty(item.Value.Field)||string.IsNullOrEmpty(item.Value.Value))
                {
                    if (!((item.Value.Operator.Equals(ConditionOperator.Null) || item.Value.Operator.Equals(ConditionOperator.NotNull)) && !string.IsNullOrEmpty(item.Value.Field)))
                    {
                        lisInvaildObligationItem.Add(item.Key);
                    }
                }
                else
                {
                    // only change value(picklist value, two options value), change opeartor when value type is datetime
                    if (!Common.DataBussiness.EntityDataHelp.VerfiyObligationValue(strCurrentEntityName, item.Value, secureEntityCache))
                    {
                        throw new Exceptions.InvalidFilterException(strCurrentEntityName, item.Value.Field, item.Value.Operator, item.Value.Value);
                    }
                }
            }
            foreach(string strInvaildKey in lisInvaildObligationItem)
            {
                dirResult.Remove(strInvaildKey);
            }
            return dirResult;
        }

        public bool IsMatch(string strKey, object objValue,NextLabs.CRMEnforcer.Log.NextLabsCRMLogs log)
        {
            bool bResult = false;
            if (strKey.Equals(this.Field))
            {
                bResult = Common.DataBussiness.EntityDataHelp.ValueCompare(objValue, this.Value, this.Operator, this.ValueType, log);
            }
            else
            {
                bResult = true;
            }
            return bResult;
        }

    }
    public class ApplySecurityFilterBasedonParentAttributesModel : ObligationModelBase
    {
        public string ParentEntityName { get; set; }
        public string RelationshipName { get; set; }
        //public string ParentAttributeName { get; set; }
        //public ConditionOperator Operator { get; set; }
        //public string Value { get; set; }
        public ApplySecurityFilterModel Filter { get; set; }
        public Common.Enums.Decision OrphanedChildRecords { get; set; }
                                                                                                                               
        private const string Obligation_Name_ApplySecurityFilterBaseOnParentAttributes = "app_sec_filter_based_on_parent_attributes";
        private const string Obligation_Attr_NamePfx_ParentRelationship = "parentrelationship";
        private const string Obligation_Attr_NamePfx_ParentEntityAttribute = "parententityattribute";
        private const string Obligation_Attr_NamePfx_OrphanedChildRecords = "orphanedchildrecords";
        private const string Obligation_Attr_NmaePfx_Operator = "operator";
        private const string Obligation_Attr_NamePrx_Value = "value";

        public ApplySecurityFilterBasedonParentAttributesModel()
        {
            Filter = new ApplySecurityFilterModel();
        }

        public static List<Dictionary<string, ApplySecurityFilterBasedonParentAttributesModel>> GetCurrentObligation( List<CEObligation> lisObligations)
        {
            List<Dictionary<string, ApplySecurityFilterBasedonParentAttributesModel>> lisResult = new List<Dictionary<string, ApplySecurityFilterBasedonParentAttributesModel>>();
            foreach (CEObligation item in lisObligations)
            {
                if (item.Get_Nmae().Equals(Obligation_Name_ApplySecurityFilterBaseOnParentAttributes))
                {
                    Dictionary<string, ApplySecurityFilterBasedonParentAttributesModel> obGeted = TransfromFromCEObligation(item);
                    if (obGeted.Count > 0)
                    {
                        lisResult.Add(obGeted);
                    }
                }
            }
            return lisResult;
        }

        private static Dictionary<string, ApplySecurityFilterBasedonParentAttributesModel> TransfromFromCEObligation(CEObligation ob)
        {
            Dictionary<string, ApplySecurityFilterBasedonParentAttributesModel> dirResult = new Dictionary<string, ApplySecurityFilterBasedonParentAttributesModel>();
            CEAttres ceAttrs = ob.GetCEAttres();
            int nCeArrtLength = ceAttrs.get_count();
            string strKey = string.Empty;
            string strValue = string.Empty;
            CEAttributeType ceType;
            for (int nIndex = 0; nIndex < nCeArrtLength; nIndex++)
            {
                ceAttrs.Get_Attre(nIndex, out strKey, out strValue, out ceType);
                if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strValue) && !strKey.Equals(strValue))
                {
                    string strRealName = string.Empty;
                    string strFlag = string.Empty;
                    if (GetObligationAttributeRealName(strKey, out strRealName, out strFlag))
                    {
                        if (!dirResult.ContainsKey(strFlag))
                        {
                            dirResult[strFlag] = new ApplySecurityFilterBasedonParentAttributesModel();
                        }
                        switch (strRealName)
                        {
                            case Obligation_Attr_NamePfx_ParentRelationship:
                                {
                                    string[] arryParentRelationship = strValue.Split(Obligation_Split_Char);
                                    if (arryParentRelationship.Length == 2)
                                    {
                                        dirResult[strFlag].ParentEntityName = arryParentRelationship[0];
                                        dirResult[strFlag].RelationshipName = arryParentRelationship[1];
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidObligationException(ob);
                                    }
                                }
                                break;
                            case Obligation_Attr_NamePfx_ParentEntityAttribute:
                                {
                                    string[] arrayParentEntityAttribute = strValue.Split(Obligation_Split_Char);
                                    if (arrayParentEntityAttribute.Length == 2)
                                    {
                                        if (dirResult[strFlag].ParentEntityName.Equals(arrayParentEntityAttribute[0], StringComparison.OrdinalIgnoreCase))
                                        {
                                            dirResult[strFlag].Filter.Field = arrayParentEntityAttribute[1];
                                        }
                                        else
                                        {
                                            throw new Exceptions.InvalidObligationException(ob);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidObligationException(ob);
                                    }
                                }
                                break;
                            case Obligation_Attr_NmaePfx_Operator:
                                {
                                    ConditionOperator conditionOperatorObligationGeted;
                                    if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.EqualsTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.Equal;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.NotEqualTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.NotEqual;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.GreaterThan, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.GreaterThan;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.LessThan, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.LessThan;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.GreaterThanOrEqualsTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.GreaterEqual;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.LessThanOrEqualsTo, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.LessEqual;
                                    }
                                    else if(strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.IsNULL, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.Null;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.IsNotNULL, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.NotNull;
                                    }
                                    else if (strValue.Equals(NextLabs.CRMEnforcer.Common.Constant.ApplySecurityFilterOblOperator.OlderThanXdays, StringComparison.OrdinalIgnoreCase))
                                    {
                                        conditionOperatorObligationGeted = ConditionOperator.OlderThanXDays;
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidObligationException(ob);
                                    }
                                    dirResult[strFlag].Filter.Operator = conditionOperatorObligationGeted;
                                }
                                break;
                            case Obligation_Attr_NamePrx_Value:
                                {
                                    dirResult[strFlag].Filter.Value = strValue;
                                }
                                break;
                            case Obligation_Attr_NamePfx_OrphanedChildRecords:
                                {
                                    Common.Enums.Decision OrphanedChild = Common.Enums.Decision.Unknow;
                                    if (!Enum.TryParse(strValue, out OrphanedChild))
                                    {
                                        throw new Exceptions.InvalidObligationException(ob);
                                    }
                                    dirResult[strFlag].OrphanedChildRecords = OrphanedChild;
                                }
                                break;
                        }
                    }
                }
            }
            List<string> lisInvaildObligationItem = new List<string>();
            foreach (KeyValuePair<string, ApplySecurityFilterBasedonParentAttributesModel> item in dirResult)
            {
                if(string.IsNullOrEmpty(item.Value.Filter.Field)
                    || string.IsNullOrEmpty(item.Value.ParentEntityName)
                    || string.IsNullOrEmpty(item.Value.RelationshipName))
                {
                    lisInvaildObligationItem.Add(item.Key);
                }
                else if (string.IsNullOrEmpty(item.Value.Filter.Value))
                {
                    if(!(item.Value.Filter.Operator.Equals(ConditionOperator.Null)|| item.Value.Filter.Operator.Equals(ConditionOperator.NotNull)))
                    {
                        lisInvaildObligationItem.Add(item.Key);
                    }
                }
            }
            foreach (string strInvaildKey in lisInvaildObligationItem)
            {
                dirResult.Remove(strInvaildKey);
            }
            return dirResult;
        }

        public static void CheckAndVefiyObligation(List<Dictionary<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel>> lisObligations, MemoryCache<SecureEntity> secureEntityCache, string strCurrentEntityName)
        {
            foreach (Dictionary<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel> item in lisObligations)
            {
                foreach (KeyValuePair<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel> keyValuePair in item)
                {
                    Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel obligation = keyValuePair.Value;
                    Common.QueryPC.CQuery.CCRMfilter crmFilter = new Common.QueryPC.CQuery.CCRMfilter(obligation);
                    string strObligationValue = obligation.Filter.Value;
                    if (!Common.DataBussiness.EntityDataHelp.VerfiyObligationValue(obligation.ParentEntityName, obligation.Filter, secureEntityCache))
                    {
                        throw new Exceptions.InvalidFilterException(obligation.ParentEntityName, obligation.Filter.Field, obligation.Filter.Operator, obligation.Filter.Value);
                    }
                }
            }
        }
    }
    public class InheritPoliciesFromModel : ObligationModelBase
    {
        private const string Obligation_Name_InheritPoliciesFrom = "inherit_policies_from";
        private const string Obligation_Attr_NamePfx_ParentEntityRelationship = "parentrelationship";
        private const string Obligation_Attr_NamePfx_OrphanedChildRecord = "orphanedchildrecords";

        public string ParentEntityLogicName { get; set; }
        public string RelationshipLogicName { get; set; }
        public Decision OrphanedChildDecision { get; set; }

        public static List<Dictionary<string, InheritPoliciesFromModel>> GetCurrentObligation(List<CEObligation> lisObligations)
        {
            List<Dictionary<string, InheritPoliciesFromModel>> lisResult = new List<Dictionary<string, InheritPoliciesFromModel>>();
            foreach (CEObligation item in lisObligations)
            {
                if (item.Get_Nmae().Equals(Obligation_Name_InheritPoliciesFrom))
                {
                    Dictionary<string, InheritPoliciesFromModel> obGeted = TransfromFromCEObligation(item);
                    if (obGeted.Count > 0)
                    {
                        lisResult.Add(obGeted);
                    }
                }
            }
            return lisResult;
        }

        private static Dictionary<string, InheritPoliciesFromModel> TransfromFromCEObligation(CEObligation ob)
        {
            Dictionary<string, InheritPoliciesFromModel> dirResult = new Dictionary<string, InheritPoliciesFromModel>();
            CEAttres ceAttrs = ob.GetCEAttres();
            int nCeArrtLength = ceAttrs.get_count();
            string strKey = string.Empty;
            string strValue = string.Empty;
            CEAttributeType ceType;
            for (int nIndex = 0; nIndex < nCeArrtLength; nIndex++)
            {
                ceAttrs.Get_Attre(nIndex, out strKey, out strValue, out ceType);
                if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strValue) && !strKey.Equals(strValue))
                {
                    string strRealName = string.Empty;
                    string strFlag = string.Empty;
                    if (GetObligationAttributeRealName(strKey, out strRealName, out strFlag))
                    {
                        if (!dirResult.ContainsKey(strFlag))
                        {
                            dirResult[strFlag] = new InheritPoliciesFromModel();
                        }
                        switch (strRealName)
                        {
                            case Obligation_Attr_NamePfx_ParentEntityRelationship:
                                {
                                    string[] arryParentRelationship = strValue.Split(Obligation_Split_Char);
                                    if (arryParentRelationship.Length == 2)
                                    {
                                        dirResult[strFlag].ParentEntityLogicName = arryParentRelationship[0];
                                        dirResult[strFlag].RelationshipLogicName = arryParentRelationship[1];
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidObligationException(ob);
                                    }
                                }
                                break;
                            case Obligation_Attr_NamePfx_OrphanedChildRecord:
                                {
                                    Enums.Decision OrphanedChild = Enums.Decision.Unknow;
                                    if (!Enum.TryParse(strValue, out OrphanedChild))
                                    {
                                        throw new Exceptions.InvalidObligationException(ob);
                                    }
                                    dirResult[strFlag].OrphanedChildDecision = OrphanedChild;
                                }
                                break;
                        }
                    }
                }
            }
            List<string> lisInvaildObligationItem = new List<string>();
            foreach (KeyValuePair<string, InheritPoliciesFromModel> item in dirResult)
            {
                if (string.IsNullOrEmpty(item.Value.ParentEntityLogicName)
                    || string.IsNullOrEmpty(item.Value.RelationshipLogicName))
                {
                    lisInvaildObligationItem.Add(item.Key);
                }
            }
            foreach (string strInvaildKey in lisInvaildObligationItem)
            {
                dirResult.Remove(strInvaildKey);
            }
            return dirResult;
        }
    }
    public class DisplayAlertMessageModel:ObligationModelBase
    {
        private const string Obligation_Name_InheritPoliciesFrom = "dp_violation_message";
        private const string Obligation_Attr_Name_Message = "message";
        public string Message { get; set; }

        public static List<DisplayAlertMessageModel> GetCurrentObligation(List<CEObligation> lisObligations)
        {
            List<DisplayAlertMessageModel> lisResult = new List<DisplayAlertMessageModel>();
            foreach (CEObligation item in lisObligations)
            {
                if (item.Get_Nmae().Equals(Obligation_Name_InheritPoliciesFrom))
                {
                    List<DisplayAlertMessageModel> obGeted = TransfromFromCEObligation(item);
                    if (obGeted.Count > 0)
                    {
                        lisResult.AddRange(obGeted);
                    }
                }
            }
            return lisResult;
        }
        private static List<DisplayAlertMessageModel> TransfromFromCEObligation(CEObligation ob)
        {
            List<DisplayAlertMessageModel> lisResult = new List<DisplayAlertMessageModel>();
            CEAttres ceAttrs = ob.GetCEAttres();
            int nCeArrtLength = ceAttrs.get_count();
            string strKey = string.Empty;
            string strValue = string.Empty;
            CEAttributeType ceType;
            for (int nIndex = 0; nIndex < nCeArrtLength; nIndex++)
            {
                ceAttrs.Get_Attre(nIndex, out strKey, out strValue, out ceType);
                if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strValue) && !strKey.Equals(strValue))
                {
                    if (strKey.Equals(Obligation_Attr_Name_Message))
                    {
                        DisplayAlertMessageModel displayAlertMessageModel = new DisplayAlertMessageModel();
                        displayAlertMessageModel.Message = strValue;
                        lisResult.Add(displayAlertMessageModel);
                    }
                }
            }
            return lisResult;
        }
    }
    public class MaskFieldModel:ObligationModelBase
    {
        private const string Obligation_Name_MaskField = "mask_fields";
        private const string Obligation_Attr_NamePfx_Column = "col";
        private const string Obligation_Attr_NamePfx_Operate = "op";
        private const string Obligation_Attr_NamePfx_Value = "val";
        private const string Obligation_Attr_Name_MaskCharacter = "mask_character";
        private const string Obligation_Attr_NamePfx_MaskField = "mask_field";

        public Dictionary<string, ApplySecurityFilterModel> DictFilters { get; set; }
        public string MaskCharacter { get; set; }
        public List<string> listMaskFields { get; set; }

        private static MaskFieldModel TransfromFromCEObligation(CEObligation ob,MemoryCache<SecureEntity> secureEntityCache,string strEntityName)
        {
            MaskFieldModel maskFieldModel = new MaskFieldModel();
            Dictionary<string, MaskFieldModel> dirResult = new Dictionary<string, MaskFieldModel>();
            maskFieldModel.DictFilters = ApplySecurityFilterModel.TransfromFromCEObligation(ob, secureEntityCache, strEntityName);
            maskFieldModel.listMaskFields = new List<string>();
            CEAttres ceAttrs = ob.GetCEAttres();
            SecureEntity currentEntityStruct = secureEntityCache.Lookup(strEntityName, x => null);
            if(currentEntityStruct==null)
            {
                throw new Exceptions.InvalidCacheException(strEntityName);
            }
            for (int nIndex=0;nIndex< ceAttrs.get_count();nIndex++)
            {
                string strKey = string.Empty;
                string strValue = string.Empty;
                CEAttributeType ceType;
                ceAttrs.Get_Attre(nIndex, out strKey, out strValue,out ceType);
                if(!string.IsNullOrEmpty(strKey)&&!string.IsNullOrEmpty(strValue)&&!strKey.Equals(strValue))
                {
                    string strRealName = string.Empty;
                    string strFlag = string.Empty;
                    if(!GetObligationAttributeRealName(strKey,out strRealName,out strFlag))
                    {
                        switch(strKey)
                        {
                            case Obligation_Attr_Name_MaskCharacter:
                                {
                                    maskFieldModel.MaskCharacter = strValue;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch(strRealName)
                        {
                            case Obligation_Attr_NamePfx_MaskField:
                                {
                                    if (currentEntityStruct.Schema.Attributes.Exists(dir => { return dir.LogicName.Equals(strValue, StringComparison.OrdinalIgnoreCase); }))
                                    {
                                        maskFieldModel.listMaskFields.Add(strValue);
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidCacheException(currentEntityStruct, string.Format("Cannot find atribute {0} on entity {1} cache when anaysis Mask Field obligation", strValue,strEntityName));
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            return maskFieldModel;
        }

        public static List<MaskFieldModel> GetCurrentObligation(List<CEObligation> lisObligations,MemoryCache<SecureEntity> entityCache,string strEntityName)
        {
            List< MaskFieldModel> lisResult = new List<MaskFieldModel>();
            foreach (CEObligation item in lisObligations)
            {
                if (item.Get_Nmae().Equals(Obligation_Name_MaskField))
                {
                    MaskFieldModel obGeted = TransfromFromCEObligation(item, entityCache,strEntityName);
                    if (obGeted.DictFilters.Count > 0 && obGeted.listMaskFields.Count > 0)
                    {
                        lisResult.Add(obGeted);
                    }
                }
            }
            return lisResult;
        }

        public static bool IsMaskFieldExist(List<CEObligation> lisObligations)
        {
            foreach (CEObligation item in lisObligations)
            {
                if (item.Get_Nmae().Equals(Obligation_Name_MaskField))
                {
                    return item.GetCEAttres().get_count() > 0 ? true : false;
                }
            }
            return false;
        }
    }
}
