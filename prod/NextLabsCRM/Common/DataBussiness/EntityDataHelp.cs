using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataBussiness.IO;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;

namespace NextLabs.CRMEnforcer.Common.DataBussiness
{
    public static class EntityDataHelp
    {
        public const string DataType_Decimal_Lowcase = "decimal";
        public const string DataType_Double_Lowcase = "double";
        public const string DataType_Integer_Lowcase = "integer";
        public const string DataType_String_Lowcase = "string";
        public const string DataType_PickList_Lowcase = "picklist";
        public const string DataType_Boolean_Lowcase = "boolean";
        public const string DataType_Lookup_Lowcase = "lookup";
        public const string DataType_Uniqueidentifier_Lowcase = "uniqueidentifier";
        public const string DataType_DateTime_Lowcase = "datetime";
        public const string DataType_BigInt_Lowcase = "bigint";
        public const string DataType_Memo_Lowcase = "memo";
        public const string DataType_State_Lowcase = "state";
        public const string DataType_Money_Lowcase = "money";
        public const string DataType_Owner_Lowcase = "owner";
        public const string DataType_Status_Lowcase = "status";
        public const string DataType_Customer_Lowcase = "customer";
        public const string DataType_EntityName_Lowcase = "entityname";

        public static object GetValueOfValue(object value)
        {
            object target = value;
            if (value != null)
            {
                if (value is OptionSetValue)
                {
                    target = (value as OptionSetValue).Value;
                }
                else if (value is EntityReference)
                {
                    target = (value as EntityReference).Name;
                }
                else if (value is Money)
                {
                    target = (value as Money).Value;
                }
                else if (value is AliasedValue)
                {
                    target = (value as AliasedValue).Value;
                }
                else if (value is DateTime)
                {
                    target = ((DateTime)value).Date;
                }
                else
                {
                    target = value.ToString();
                }
            }
            return target;
        }
        public static bool ValueCompare(object objValue1, object objValue2, ConditionOperator operation, Type filterValueType, NextLabsCRMLogs log)
        {
            ICompare valueComPare = null;
            if (filterValueType == typeof(Decimal))
            {
                valueComPare = new DecimalCompare(log);
            }
            else if (filterValueType == typeof(Double))
            {
                valueComPare = new DoubleCompare(log);
            }
            else if (filterValueType == typeof(Int64))
            {
                valueComPare = new Int64Compare(log);
            }
            else if (filterValueType == typeof(int))
            {
                valueComPare = new IntCompare(log);
            }
            else if (filterValueType == typeof(Boolean))
            {
                valueComPare = new BooleanCompare(log);
            }
            else if (filterValueType == typeof(DateTime))
            {
                valueComPare = new DateTimeCompare(log);
            }
            else
            {
                valueComPare = new StringCompare(log);
            }

            return valueComPare.Compare(objValue1, operation, objValue2);
        }

        //public static EntityCollection RetrieveMultiple(IOrganizationService service, string strEntityLocalName, List<string> listGuidRecordId, string[] arrayCloumnName)
        //{
        //    RetrieveMultipleRequest retrieveRequest = new RetrieveMultipleRequest();
        //    retrieveRequest.RequestId = NXLIDProvider.CreateNewNXLGUID();
        //    QueryExpression query = new QueryExpression();
        //    query.EntityName = strEntityLocalName;
        //    query.ColumnSet = new ColumnSet(arrayCloumnName);
        //    query.Criteria.AddCondition(strEntityLocalName + "id", ConditionOperator.In, listGuidRecordId.ToArray());
        //    RetrieveMultipleResponse response= (RetrieveMultipleResponse)service.Execute(retrieveRequest);
        //    return response.EntityCollection;
        //}

        public static object RertieveSingle(IOrganizationService service, string strEntityLocalName, Guid guidRecordId, string strCloumnName)
        {
            object oResult = null;
            try
            {
                RetrieveRequest retrieveRequest = new RetrieveRequest();
                retrieveRequest.ColumnSet = new ColumnSet(strCloumnName);
                retrieveRequest.RequestId = NXLIDProvider.CreateNewNXLGUID();
                retrieveRequest.Target = new EntityReference(strEntityLocalName, guidRecordId);
                Entity entityRertivev = (Entity)((RetrieveResponse)service.Execute(retrieveRequest)).Entity;
                if (entityRertivev != null)
                {
                    if (entityRertivev.Attributes != null)
                    {
                        if (entityRertivev.Attributes.ContainsKey(strCloumnName))
                        {
                            oResult = entityRertivev.Attributes[strCloumnName];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("NextLabs.CRMEnforcer.Common.DataBussiness.EntityDataHelp.RertieveSingle Faild: " + ex.Message, ex);
            }
            return oResult;
        }

        public static AttributeCollection RertieveSingle(IOrganizationService service, string strEntityLocalName, Guid guidRecordId, ColumnSet columnSet)
        {
            AttributeCollection result = null;
            Guid? requestId = NXLIDProvider.CreateNewNXLGUID(); 
            try
            {
                RetrieveRequest retrieveRequest = new RetrieveRequest();
                retrieveRequest.ColumnSet = columnSet;
                retrieveRequest.RequestId = requestId;
                retrieveRequest.Target = new EntityReference(strEntityLocalName, guidRecordId);
                Entity entityRertivev = (Entity)((RetrieveResponse)service.Execute(retrieveRequest)).Entity;

                if (entityRertivev != null)
                {
                    if (entityRertivev.Attributes != null)
                    {
                        result = entityRertivev.Attributes;
                    }
                }
            }
            catch (Exception ex)
            {
                string strColumnSet = string.Empty;
                if (columnSet == null || columnSet.Columns== null || columnSet.Columns.Count == 0)
                {
                    strColumnSet = ";columnset is empty or null;";
                }
                else
                {
                    foreach (string column in columnSet.Columns)
                    {
                        strColumnSet += column + ",";
                    }
                    strColumnSet += ";";
                }

                string strRecordID = string.Empty;
                if(guidRecordId == null)
                {
                    strRecordID = ";record id is null;";
                }
                else
                {
                    strRecordID = guidRecordId.ToString();
                }

                string strParamsInput = string.Format("requestid is {0}; entity is {1};recordid is {2},columns are {3}", requestId.ToString(),strEntityLocalName, strRecordID, strColumnSet);
                throw new Exception("NextLabs.CRMEnforcer.Common.DataBussiness.EntityDataHelp.RetriveSingle Faild,param is : "+ strParamsInput +
                    ";message is:"+ ex.Message, ex);
            }
            return result;
        }

        public static Entity RertieveSingleEntity(IOrganizationService service, string strEntityLocalName, Guid guidRecordId, ColumnSet columnSet)
        {
            Guid? requestId = NXLIDProvider.CreateNewNXLGUID();
            try
            {
                RetrieveRequest retrieveRequest = new RetrieveRequest();
                retrieveRequest.ColumnSet = columnSet;
                retrieveRequest.RequestId = requestId;
                retrieveRequest.Target = new EntityReference(strEntityLocalName, guidRecordId);
                Entity entityRertivev = (Entity)((RetrieveResponse)service.Execute(retrieveRequest)).Entity;

                return entityRertivev;
            }
            catch (Exception ex)
            {
                string strColumnSet = string.Empty;
                if (columnSet == null || columnSet.Columns == null || columnSet.Columns.Count == 0)
                {
                    strColumnSet = ";columnset is empty or null;";
                }
                else
                {
                    foreach (string column in columnSet.Columns)
                    {
                        strColumnSet += column + ",";
                    }
                    strColumnSet += ";";
                }

                string strRecordID = string.Empty;
                if (guidRecordId == null)
                {
                    strRecordID = ";record id is null;";
                }
                else
                {
                    strRecordID = guidRecordId.ToString();
                }

                string strParamsInput = string.Format("requestid is {0}; entity is {1};recordid is {2},columns are {3}", requestId.ToString(), strEntityLocalName, strRecordID, strColumnSet);
                throw new Exception("NextLabs.CRMEnforcer.Common.DataBussiness.EntityDataHelp.RetriveSingle Faild,param is : " + strParamsInput +
                    ";message is:" + ex.Message, ex);
            }
        }

        public static AttributeCollection RertieveSingle(AttributeCollection sourceAttrs, ColumnSet columnSet)
        {
            AttributeCollection result = null;
            if (columnSet.AllColumns)
            {
                result = sourceAttrs;
            }
            else
            {
                result = new AttributeCollection();
                foreach (KeyValuePair<string, object> item in sourceAttrs)
                {
                    if (columnSet.Columns.ToList<string>().Exists(dir => { return dir.Equals(item.Key, StringComparison.OrdinalIgnoreCase); }))
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        public static EntityCollection RetrieveMultiple(IOrganizationService service, QueryBase query)
        {
            RetrieveMultipleRequest retrieveRequest = new RetrieveMultipleRequest();
            retrieveRequest.Query = query;

#if DEBUG
            //if (query is QueryExpression)
            //{
            //    FetchExpression fe = Util.QueryExpressionToFetchExpression(service, query as QueryExpression);
            //}
#endif
            retrieveRequest.RequestId = NXLIDProvider.CreateNewNXLGUID();
            EntityCollection ec = ((RetrieveMultipleResponse)service.Execute(retrieveRequest)).EntityCollection;

            return ec;
        }

        private static string ConvertStringableValueOrListToString(object obj, ref CEAttributeType attrType)
        {
            //lock (obj)
            {
                if (obj is List<object>)
                {
                    List<object> objList = (List<object>)obj;
                    attrType = CEAttributeType.NXL_List;
                    return string.Join(CEAttribute.SperaterForListValue, objList.ToArray());
                }

                return obj.ToString();
            }
        }

        private static string ConvertLookupValueOrListToString(object obj, ref CEAttributeType attrType)
        {
            string strValue = string.Empty;
            //lock (obj)
            {
                if (obj is List<object>)
                {
                    List<object> results = new List<object>();
                    List<object> objList = (List<object>)obj;
                    foreach (object objItem in objList)
                    {
                        EntityReference tempEntityRef = (EntityReference)objItem;
                        results.Add(tempEntityRef.Name);
                        attrType = CEAttributeType.NXL_List;
                    }
                    strValue = string.Join(CEAttribute.SperaterForListValue, results.ToArray());
                }
                else if (obj is EntityReference)
                {
                    EntityReference entityRef = (EntityReference)obj;
                    strValue = entityRef.Name;
                    attrType = CEAttributeType.XACML_String;
                }
            }
            return strValue;
        }

        private static string ConvertMonyValueOrListToString(object obj, ref CEAttributeType attrType)
        {
            string strResult = string.Empty;
            //lock (obj)
            {
                if (obj is List<object>)
                {
                    List<object> results = new List<object>();
                    List<object> objList = (List<object>)obj;
                    foreach (object objItem in objList)
                    {
                        Money money = (Money)objItem;
                        results.Add(money.Value.ToString());
                        attrType = CEAttributeType.NXL_List;
                    }
                    strResult = string.Join(CEAttribute.SperaterForListValue, results.ToArray());
                }
                else
                {
                    Money moneyObj = (Money)obj;
                    strResult = moneyObj.Value.ToString();
                    attrType = CEAttributeType.XACML_Double;
                }
            }
            return strResult;
        }

        private static string ConvertBoolValueToString(DataModel.MetaData.Attribute attrCache, SecureEntity entityStruct, string attribteName, object obj, ref CEAttributeType attrType)
        {
            if (obj is Boolean)
            {
                DataModel.MetaData.Option option = attrCache.Options.Find(dir => { return dir.Value.Equals((Boolean)obj ? 1 : 0); });
                if (option != null)
                {
                    attrType = CEAttributeType.XACML_String;
                    return option.Label;
                }

                throw new Exceptions.InvalidCacheException(entityStruct, string.Format("cannot find attribute  {0} options struct on memory cache", attribteName));
            }
            throw new Exceptions.InvalidCacheException(entityStruct, string.Format("This find attribute  {0} type struct on memory cache is incorrect {1}", attribteName, obj.GetType()));

        }
        private static string ConvertBoolValueOrListToString(DataModel.MetaData.Attribute attrCache, SecureEntity entityStruct, string attribteName, object obj, ref CEAttributeType attrType)
        {
            
            string strResult = string.Empty;
            //lock (obj)
            {
                if (obj is List<object>)
                {
                    List<object> results = new List<object>();
                    List<object> objList = (List<object>)obj;
                    foreach (object objItem in objList)
                    {
                        results.Add(ConvertBoolValueToString(attrCache, entityStruct, attribteName, objItem, ref attrType));
                        attrType = CEAttributeType.NXL_List;
                    }
                    strResult = string.Join(CEAttribute.SperaterForListValue, results.ToArray());
                }
                else
                {
                    strResult = ConvertBoolValueToString(attrCache, entityStruct, attribteName, obj, ref attrType);
                }
            }
            return strResult;
        }


        private static string ConvertPickListValueToString(DataModel.MetaData.Attribute attrCache, SecureEntity entityStruct, string attribteName, object obj, ref CEAttributeType attrType)
        {

            if (obj is OptionSetValue)
            {
                //lock (obj)
                {
                    DataModel.MetaData.Option option = attrCache.Options.Find(dir => { return dir.Value.Equals(((OptionSetValue)obj).Value); });
                    if (option != null)
                    {
                        attrType = CEAttributeType.XACML_String;
                        return option.Label;
                    }
                }

                throw new Exceptions.InvalidCacheException(entityStruct, string.Format("cannot find attribute  {0} options struct on memory cache", attribteName));
            }

            throw new Exceptions.InvalidCacheException(entityStruct, string.Format("This find attribute  {0} type struct on memory cache is incorrect {1}", attribteName, obj.GetType()));

        }

        private static string ConvertPickListValueOrListToString(DataModel.MetaData.Attribute attrCache, SecureEntity entityStruct, string attribteName, object obj, ref CEAttributeType attrType)
        {
            string strResult = string.Empty;
            //lock (obj)
            {
                if (obj is List<object>)
                {
                    List<object> results = new List<object>();
                    List<object> objList = (List<object>)obj;
                    foreach (object objItem in objList)
                    {
                        results.Add(ConvertPickListValueToString(attrCache, entityStruct, attribteName, objItem, ref attrType));
                        attrType = CEAttributeType.NXL_List;
                    }
                    strResult = string.Join(CEAttribute.SperaterForListValue, results.ToArray());
                }
                else
                {
                    strResult = ConvertPickListValueToString(attrCache, entityStruct, attribteName, obj, ref attrType);
                }
            }
            return strResult;
        }

        //public static AttributeCollection GetClonedAttribute(this SystemUser user)
        //{
        //    AttributeCollection result = new AttributeCollection();
        //    KeyValuePair<string, object>[] arrayAttribute = new KeyValuePair<string, object>[user.Attributes.Count];
        //    user.Attributes.CopyTo(arrayAttribute, user.Attributes.Count);
        //    result.AddRange(arrayAttribute);
        //    return result;
        //}

        public static CEAttribute TransformToCEAttribute(/*MemoryCache<SecureEntity> entityStructCache*/SecureEntity entityStruct,
            string strEntityName, string strAttrName, object objAttrValue, Log.NextLabsCRMLogs log)
        {
            string strAttrKey = strAttrName;
            string strAttrValue = null;
            CEAttributeType cetype = CEAttributeType.XACML_AnyURI;
            if (entityStruct == null)
            {
                throw new NullReferenceException("EntityDataHelp.TransformToRealValue must receive a not null SecureEntity");
            }
            if (string.IsNullOrEmpty(strEntityName))
            {
                throw new NullReferenceException("EntityDataHelp.TransformToRealValue must receive a not null entity name");
            }
            if (string.IsNullOrEmpty(strAttrName))
            {
                throw new NullReferenceException("EntityDataHelp.TransformToRealValue must receive a not null attribute name");
            }
            if (objAttrValue == null)
            {
                throw new NullReferenceException("EntityDataHelp.TransformToRealValue must receive a not null attribute Value");
            }
            //SecureEntity entityStruct = entityStructCache.Lookup(strEntityName, x => null);

            if (entityStruct != null)
            {
                if (entityStruct.Secured)
                {
                    DataModel.MetaData.Attribute attrCache = entityStruct.Schema.Attributes.Find(dir => { return dir.LogicName.Equals(strAttrName); });
                    if (attrCache != null)
                    {
                        switch (attrCache.DataType.ToLower())
                        {
                            case DataType_Money_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_Double;
                                    strAttrValue = ConvertMonyValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_Decimal_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_Double;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_Double_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_Double;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_Integer_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_Integer;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);

                                }
                                break;
                            case DataType_BigInt_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_Integer;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);

                                }
                                break;
                            case DataType_String_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_String;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_Boolean_Lowcase:
                                {
                                    if (attrCache.Options != null && attrCache.Options.Count > 0)
                                    {
                                        cetype = CEAttributeType.XACML_String;
                                        strAttrValue = ConvertBoolValueOrListToString(attrCache, entityStruct, strAttrName, objAttrValue, ref cetype);
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidCacheException(entityStruct, string.Format("cannot find attribute  {0} options struct on memory cache", strAttrName));
                                    }
                                }
                                break;
                            case DataType_EntityName_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_String;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_Status_Lowcase:
                            case DataType_State_Lowcase:
                            case DataType_PickList_Lowcase:
                                {
                                    if (attrCache.Options != null && attrCache.Options.Count > 0)
                                    {
                                        cetype = CEAttributeType.XACML_String;
                                        strAttrValue = ConvertPickListValueOrListToString(attrCache, entityStruct, strAttrName, objAttrValue, ref cetype);
                                    }
                                    else
                                    {
                                        throw new Exceptions.InvalidCacheException(entityStruct, string.Format("cannot find attribute  {0} options struct on memory cache", strAttrName));
                                    }
                                }
                                break;
                            case DataType_Owner_Lowcase:
                            case DataType_Lookup_Lowcase:
                            case DataType_Customer_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_String;
                                    strAttrValue = ConvertLookupValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_Uniqueidentifier_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_String;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_DateTime_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_String;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            case DataType_Memo_Lowcase:
                                {
                                    cetype = CEAttributeType.XACML_String;
                                    strAttrValue = ConvertStringableValueOrListToString(objAttrValue, ref cetype);
                                }
                                break;
                            default:
                                throw new Exceptions.InvalidDataTypeException("EntityDataHelp", "TransformToCEAttribute", attrCache.DataType);
                                break;
                        }
                    }
                    else
                    {
                        if (!strAttrKey.Equals(entityStruct.Schema.PrimaryIDName, StringComparison.OrdinalIgnoreCase))
                        {
                            log.WriteLog(Enums.LogLevel.Warning, string.Format("cannot find attribute {0} struct on memory cache", strAttrName));
                            return null;
                        }
                        else
                        {
                            log.WriteLog(Enums.LogLevel.Debug, string.Format("cannot find attribute {0} struct on memory cache , but it is entity {1} primay key, ignore this error", strAttrName, strEntityName));
                        }
                    }
                }
                else
                {
                    throw new Exceptions.InvalidCacheException(strEntityName);
                }
            }
            else
            {
                throw new Exceptions.InvalidCacheException(strEntityName);
            }
            return new CEAttribute(strAttrKey, strAttrValue, cetype);
        }

        public static bool VerfiyObligationValue(string strEntityName, ApplySecurityFilterModel filter, MemoryCache<SecureEntity> secureEntityCache)
        {
            bool bResult = false;
            SecureEntity secureEntityCurrent = secureEntityCache.Lookup(strEntityName, x => null);
            if (secureEntityCurrent != null)
            {
                string strFilterColumn = filter.Field;
                Common.DataModel.MetaData.Attribute attribute = secureEntityCurrent.Schema.Attributes.Find(dir => { return dir.LogicName.Equals(strFilterColumn, StringComparison.OrdinalIgnoreCase); });
                if (attribute != null && attribute.DataType != null)
                {
                    if (filter.Operator.Equals(ConditionOperator.Null) || filter.Operator.Equals(ConditionOperator.NotNull))
                    {
                        bResult = true;
                    }
                    else
                    {
                        if (!CheckOperator(attribute.DataType, filter.Operator))
                        {
                            throw new Exceptions.InvalidFilterException(attribute.DataType + " this attribute cannot support");
                        }
                        switch (attribute.DataType.ToLower())
                        {
                            case DataType_DateTime_Lowcase:
                                {
                                    if (filter.Operator.Equals(ConditionOperator.OlderThanXDays))
                                    {
                                        int nTemp;
                                        if (Int32.TryParse(filter.Value, out nTemp))
                                        {
                                            filter.Value = DateTime.Now.AddDays(-(nTemp + 1)).Date.ToString();
                                            filter.Operator = ConditionOperator.OnOrBefore;
                                            filter.ValueType = typeof(DateTime);
                                            bResult = true;
                                        }
                                    }
                                    else
                                    {
                                        DateTime dtTemp;
                                        if (DateTime.TryParse(filter.Value, out dtTemp))
                                        {
                                            filter.ValueType = typeof(DateTime);
                                            switch (filter.Operator)
                                            {
                                                case ConditionOperator.Equal:
                                                    filter.Operator = ConditionOperator.On;
                                                    break;
                                                case ConditionOperator.GreaterEqual:
                                                    filter.Operator = ConditionOperator.OnOrAfter;
                                                    break;
                                                case ConditionOperator.LessEqual:
                                                    filter.Operator = ConditionOperator.OnOrBefore;
                                                    break;
                                            }
                                            bResult = true;
                                        }
                                    }
                                }
                                break;
                            case DataType_String_Lowcase:
                                {
                                    filter.ValueType = typeof(String);
                                    bResult = true;
                                }
                                break;
                            case DataType_Memo_Lowcase:
                                {
                                    filter.ValueType = typeof(String);
                                    bResult = true;
                                }
                                break;
                            case DataType_Money_Lowcase:
                            case DataType_Decimal_Lowcase:
                                {
                                    filter.ValueType = typeof(Decimal);
                                    Decimal digit = 0;
                                    if (Decimal.TryParse(filter.Value, out digit))
                                    {
                                        bResult = true;
                                    }
                                }
                                break;
                            case DataType_Double_Lowcase:
                                {
                                    filter.ValueType = typeof(Double);
                                    Double digit = 0;
                                    if (Double.TryParse(filter.Value, out digit))
                                    {
                                        bResult = true;
                                    }
                                }
                                break;
                            case DataType_BigInt_Lowcase:
                                {
                                    filter.ValueType = typeof(Int64);
                                    Int64 digit = 0;
                                    if (Int64.TryParse(filter.Value, out digit))
                                    {
                                        bResult = true;
                                    }
                                }
                                break;
                            case DataType_Integer_Lowcase:
                                {
                                    filter.ValueType = typeof(int);
                                    int digit = 0;
                                    if (int.TryParse(filter.Value, out digit))
                                    {
                                        bResult = true;
                                    }
                                }
                                break;
                            case DataType_Boolean_Lowcase:
                                {
                                    filter.ValueType = typeof(Boolean);
                                    if (attribute.Options != null && attribute.Options.Count != 0)
                                    {
                                        bool bFind = false;
                                        foreach (Common.DataModel.MetaData.Option option in attribute.Options)
                                        {
                                            if (option.Label != null)
                                            {
                                                if (option.Label.Equals(filter.Value, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    filter.Value = option.Value == 0 ? "false" : "true";
                                                    bFind = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (bFind)
                                        {
                                            bResult = true;
                                        }
                                    }
                                }
                                break;
                            case DataType_State_Lowcase:
                            case DataType_Status_Lowcase:
                            case DataType_PickList_Lowcase:
                                {
                                    filter.ValueType = typeof(string);
                                    if (attribute.Options != null && attribute.Options.Count != 0)
                                    {
                                        bool bFind = false;
                                        foreach (Common.DataModel.MetaData.Option option in attribute.Options)
                                        {
                                            if (option.Label != null)
                                            {
                                                if (option.Label.Equals(filter.Value, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    filter.Value = option.Value.ToString();
                                                    bFind = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (bFind)
                                        {
                                            bResult = true;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    throw new Exceptions.InvalidCacheException(secureEntityCurrent, "Cannot find " + filter.Field + " at entity " + strEntityName + " Secure Entity Cache");
                }
            }
            else
            {
                throw new Exceptions.InvalidCacheException(strEntityName);
            }
            return bResult;
        }

        private static bool CheckOperator(string strDateType, ConditionOperator conditionOperator)
        {
            bool bResult = false;
            if (strDateType.ToLower().Equals(DataType_Decimal_Lowcase)
                    || strDateType.ToLower().Equals(DataType_Double_Lowcase)
                    || strDateType.ToLower().Equals(DataType_Integer_Lowcase)
                    || strDateType.ToLower().Equals(DataType_BigInt_Lowcase)
                    || strDateType.ToLower().Equals(DataType_Money_Lowcase))
            {
                if (conditionOperator.Equals(ConditionOperator.LessThan)
                        || conditionOperator.Equals(ConditionOperator.GreaterEqual)
                        || conditionOperator.Equals(ConditionOperator.NotEqual)
                        || conditionOperator.Equals(ConditionOperator.LessEqual)
                        || conditionOperator.Equals(ConditionOperator.Equal)
                        || conditionOperator.Equals(ConditionOperator.GreaterThan))
                {
                    bResult = true;
                }
            }
            else if (strDateType.ToLower().Equals(DataType_DateTime_Lowcase))
            {
                if (conditionOperator.Equals(ConditionOperator.GreaterEqual)
                    || conditionOperator.Equals(ConditionOperator.LessEqual)
                     || conditionOperator.Equals(ConditionOperator.Equal)
                     || conditionOperator.Equals(ConditionOperator.OlderThanXDays)
                    //fetch xml don't define not on
                    //|| conditionOperator.Equals(ConditionOperator.NotEqual)
                    )
                {
                    bResult = true;
                }
            }
            else if (strDateType.ToLower().Equals(DataType_String_Lowcase)
                || strDateType.ToLower().Equals(DataType_PickList_Lowcase)
                || strDateType.ToLower().Equals(DataType_Boolean_Lowcase)
                || strDateType.ToLower().Equals(DataType_Memo_Lowcase)
                || strDateType.ToLower().Equals(DataType_Uniqueidentifier_Lowcase)
                || strDateType.ToLower().Equals(DataType_Lookup_Lowcase)
                || strDateType.ToLower().Equals(DataType_EntityName_Lowcase)
                || strDateType.ToLower().Equals(DataType_Customer_Lowcase)
                || strDateType.ToLower().Equals(DataType_State_Lowcase)
                || strDateType.ToLower().Equals(DataType_Owner_Lowcase)
                || strDateType.ToLower().Equals(DataType_Status_Lowcase))
            {
                if (conditionOperator.Equals(ConditionOperator.Equal)
                       || conditionOperator.Equals(ConditionOperator.NotEqual))
                {
                    bResult = true;
                }
            }
            else
            {
                throw new Exceptions.InvalidFilterException("EntityDataHelp.CheckOperator Cannot find this type:" + strDateType);
            }
            return bResult;
        }

        public static ColumnSet GetEntityColumn(string strEntityName, MemoryCache<SecureEntity> secureEntityCache)
        {
            ColumnSet result = new ColumnSet();
            SecureEntity secureEntity = secureEntityCache.Lookup(strEntityName, x => null);
            if (secureEntity != null)
            {
                foreach (Common.DataModel.MetaData.Attribute item in secureEntity.Schema.Attributes)
                {
                    if (!item.LogicName.Equals(Common.Constant.AttributeKeyName.NXL_ISOwner) && !item.LogicName.Equals(Common.Constant.AttributeKeyName.NXL_ISShared))
                    {
                        result.Columns.Add(item.LogicName);
                    }
                }
            }
            else
            {
                throw new Exceptions.InvalidCacheException(strEntityName);
            }
            return result;
        }

        public static List<string> GetEntityColumn(object objQuery)
        {
            List<string> lisResult = new List<string>();
            if (objQuery is FetchExpression)
            {
                FetchExpression fetchExpress = objQuery as FetchExpression;
                string strSourceFetchXml = fetchExpress.Query;
                IStringSerialize xmlSericaliehelp = new XMLSerializeHelper();
                FetchType fetchInstance = xmlSericaliehelp.Deserialize(typeof(FetchType), strSourceFetchXml) as FetchType;

                int fetchEntityIndex = FilterUtil.FindTopEntityIndex(fetchInstance);
                if (fetchEntityIndex > -1)
                {
                    FetchEntityType fetchEntity = (FetchEntityType)fetchInstance.Items[fetchEntityIndex];
                    foreach (object item in fetchEntity.Items)
                    {
                        if (item is FetchAttributeType)
                        {
                            lisResult.Add((item as FetchAttributeType).name);
                        }
                    }
                }
            }
            else if (objQuery is QueryExpression)
            {
                QueryExpression queryExcpress = objQuery as QueryExpression;
                foreach (string strColumn in queryExcpress.ColumnSet.Columns)
                {
                    lisResult.Add(strColumn);
                }
            }
            return lisResult;
        }

        public static void AppendExtendAttributesToUserAttributes(
            AttributeCollection attrs,
            List<Entity> entities)
        {
            if (attrs == null || entities == null)
            {
                return;
            }

            foreach (Entity entity in entities)
            {
                foreach (KeyValuePair<string, object> attr in entity.Attributes)
                {
                    string keyNameWithPrefix = entity.LogicalName + "-" + attr.Key;
                    if (!attrs.ContainsKey(keyNameWithPrefix))
                    {
                        attrs[keyNameWithPrefix] = new List<object>();
                    }
                    ((List<object>)(attrs[entity.LogicalName + "-" + attr.Key])).Add(attr.Value);
                }
            }
        }

        public static Dictionary<string, List<Guid>> GetSharedRecordIDs(IOrganizationService service, MemoryCache<SecureEntity> entityMetadata, Guid initUserId)
        {
            SharedRecordRequest request = new SharedRecordRequest(service, entityMetadata, initUserId);
            SharedRecordResponse response = request.Execute();
            return response.GetRecordIDs();
        }

        public static bool? IsRecordShared(
            AttributeCollection attrs,
            string strCurrentEntityName,
            MemoryCache<SecureEntity> secureEntityCache,
            Guid recordId,
            Dictionary<string, List<Guid>> sharedRecords)
        {

            if (!Util.IsIsSharedEnabled(strCurrentEntityName, secureEntityCache))
            {
                return null;
            }

            bool isShared = false;

            if (sharedRecords != null && sharedRecords.ContainsKey(strCurrentEntityName))
            {
                List<Guid> records = sharedRecords[strCurrentEntityName];
                if (records.Contains(recordId))
                {
                    isShared = true;
                }
            }

            return isShared;
        }
        public static bool? AppendIsSharedFakeAttribute(
            AttributeCollection attrs,
            string strCurrentEntityName,
            MemoryCache<SecureEntity> secureEntityCache,
            Guid recordId,
            Dictionary<string, List<Guid>> sharedRecords)
        {
            if (!Util.IsIsSharedEnabled(strCurrentEntityName, secureEntityCache))
            {
                return null;
            }

            bool? isShared = IsRecordShared(attrs, strCurrentEntityName, secureEntityCache, recordId, sharedRecords);

            if (isShared != null)
            {
                AddItem2AttributeCollection(Common.Constant.AttributeKeyName.NXL_ISShared, isShared.Value, attrs);
            }
            return isShared;
        }
        public static void AppendIsSharedFakeAttribute(IOrganizationService service,
            AttributeCollection attrs,
            string strCurrentEntityName,
            MemoryCache<SecureEntity> secureEntityCache,
            Guid initUserId,
            Guid recordId)
        {
            if (!Util.IsIsSharedEnabled(strCurrentEntityName, secureEntityCache))
            {
                return;
            }

            SecureEntity entityMetaData = secureEntityCache.Lookup(strCurrentEntityName, x => null);
            if (entityMetaData == null)
            {
                return;
            }

            SharedRecordRequest request = new SharedRecordRequest(service, secureEntityCache, initUserId);
            SharedRecordResponse response = request.Execute();
            List<Guid> records = response.GetRecordIDs(strCurrentEntityName);
            bool isShared = false;
            if (records != null)
            {
                if (records.Contains<Guid>(recordId))
                {
                    isShared = true;
                }
            }

            AddItem2AttributeCollection(Common.Constant.AttributeKeyName.NXL_ISShared, isShared, attrs);
        }

        public static bool? IsCurrentUserOwner(
            AttributeCollection attrs,
            string strCurrentEntityName,
            MemoryCache<SecureEntity> entityAttrsStruct,
            Guid initUserId,
            List<Guid> teamIDs)
        {
            if (!Util.IsIsOwnerEnabled(strCurrentEntityName, entityAttrsStruct))
            {
                return null;
            }

            if (!attrs.ContainsKey(Common.Constant.AttributeKeyName.OwnerId))
            {
                return null;
            }

            Guid ownerId = ((EntityReference)attrs[Common.Constant.AttributeKeyName.OwnerId]).Id;
            bool isOwner = false;
            if (initUserId.Equals(ownerId))
            {
                isOwner = true;
            }
            else
            {
                if (teamIDs != null && teamIDs.Contains<Guid>(ownerId))
                {
                    isOwner = true;
                }
            }

            return isOwner;
        }


        public static bool? AppendIsOwnerFakeAttribute(
            AttributeCollection attrs,
            string strCurrentEntityName,
            MemoryCache<SecureEntity> entityAttrsStruct,
            Guid initUserId,
            List<Guid> teamIDs)
        {
            bool? isOwner = null;
            if ((isOwner = IsCurrentUserOwner(attrs, strCurrentEntityName, entityAttrsStruct, initUserId, teamIDs)) != null)
            {
                AddItem2AttributeCollection(Common.Constant.AttributeKeyName.NXL_ISOwner, isOwner.Value, attrs);
            }
            return isOwner;
        }


        public static List<Guid> GetUserTeamIDs(IOrganizationService service, Guid initUserId)
        {
            List<Guid> teamIDs = null;
            UserTeamIDRequest request = new UserTeamIDRequest(service, initUserId);
            IRecordIDResponse response = request.Execute();
            teamIDs = response.GetRecordIDs();
            return teamIDs;
        }

        public static void GetTeamsForUserAttribute(
            IOrganizationService service,
            Guid initUserId,
            MemoryCache<SecureEntity> metadataCache,
            ref List<Guid> teamIDs, ref List<Entity> teams)
        {
            SecureEntity teamMetadata = metadataCache.Lookup(Team.EntityLogicalName, x => null);
            if (teamMetadata == null)
            {
                return;
            }
            GetTeamsForUserAttribute(service, initUserId, teamMetadata, ref teamIDs, ref teams);
        }

        public static void GetTeamsForUserAttribute(
            IOrganizationService service,
            Guid initUserId,
            SecureEntity teamMetadata,
            ref List<Guid> teamIDs, ref List<Entity> teams)
        {
            if (service == null || teamMetadata == null)
            {
                return;
            }
            UserTeamRequest request = new UserTeamRequest(service, initUserId, teamMetadata);
            IRecordResponse response = request.Execute();
            teamIDs = response.GetRecordIDs();
            teams = response.GetRecords();
        }


        public static void GetRolesForUserAttribute(
           IOrganizationService service,
           Guid initUserId,
           MemoryCache<SecureEntity> metadataCache,
           ref List<Entity> roles)
        {
            SecureEntity roleMetadata = metadataCache.Lookup(Role.EntityLogicalName, x => null);
            if (roleMetadata == null)
            {
                return;
            }
            GetRolesForUserAttribute(service, initUserId, roleMetadata, ref roles);
        }

        public static void GetRolesForUserAttribute(IOrganizationService service,
            Guid initUserId,
            SecureEntity roleMetadata,
            ref List<Entity> roles)
        {
            if (service == null || roleMetadata == null)
            {
                return;
            }
            UserRoleRequest request = new UserRoleRequest(service, initUserId, roleMetadata);
            IRecordResponse response = request.Execute();
            roles = response.GetRecords();
        }

        private static void AddItem2AttributeCollection(string key, object value, AttributeCollection collection)
        {
            if(!collection.Contains(key))
            {
                collection.Add(new KeyValuePair<string, object>(key, value));
            }
            else
            {
                collection[key] = value;
            }
        }
        public static List<Guid> AppendIsOwnerFakeAttribute(IOrganizationService service,
            AttributeCollection attrs,
            string strCurrentEntityName,
            MemoryCache<SecureEntity> entityAttrsStruct,
            Guid initUserId)
        {
            if (!Util.IsIsOwnerEnabled(strCurrentEntityName, entityAttrsStruct))
            {
                return null;
            }

            if (!attrs.ContainsKey(Common.Constant.AttributeKeyName.OwnerId))
            {
                return null;
            }

            Guid ownerId = ((EntityReference)attrs[Common.Constant.AttributeKeyName.OwnerId]).Id;
            List<Guid> teamids = null;
            if (initUserId.Equals(ownerId))
            {
                AddItem2AttributeCollection(Common.Constant.AttributeKeyName.NXL_ISOwner, true, attrs);
            }
            else
            {
                UserTeamIDRequest request = new UserTeamIDRequest(service, initUserId);
                IRecordIDResponse response = request.Execute();
                teamids = response.GetRecordIDs();

                if (teamids.Contains<Guid>(ownerId))
                {
                    AddItem2AttributeCollection(Common.Constant.AttributeKeyName.NXL_ISOwner, true, attrs);
                }
                else
                {
                    AddItem2AttributeCollection(Common.Constant.AttributeKeyName.NXL_ISOwner, false, attrs);
                }
            }

            return teamids;
        }

        public static List<Guid> GetParentEntityIds(IOrganizationService service, string strObligationParentEntityName, Obligation.Relation relation, object attributeValue, Log.NextLabsCRMLogs log)
        {
            List<Guid> lisResult = new List<Guid>();
            if (relation is Obligation.N1Relation)
            {
                if (attributeValue is EntityReference)
                {
                    EntityReference entityRef = attributeValue as EntityReference;
                    if (entityRef.LogicalName == strObligationParentEntityName)
                    {
                        lisResult.Add((attributeValue as EntityReference).Id);
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Warning, string.Format("Obligation definite parnet entity name {0}, but the record reference entity name {1}", strObligationParentEntityName, entityRef.LogicalName));
                    }
                }
                else
                {
                    throw new Exception(string.Format("attribute {0} type is not Entity Reference, cannot get parent entity record", relation.CurrentAttribute));
                }
            }
            else if (relation is Obligation.NNRelation)
            {
                if (attributeValue is Guid)
                {
                    Obligation.NNRelation nnRelation = relation as Obligation.NNRelation;
                    QueryExpression queryGetIntersectEntity = new QueryExpression(nnRelation.IntersectEntity);
                    queryGetIntersectEntity.ColumnSet = new ColumnSet(nnRelation.IntersectAttributeFrom, nnRelation.IntersectAttributeTo);
                    queryGetIntersectEntity.Criteria.AddCondition(nnRelation.IntersectAttributeFrom, ConditionOperator.Equal, attributeValue.ToString());
                    EntityCollection entityGetdIntersect = EntityDataHelp.RetrieveMultiple(service, queryGetIntersectEntity);

                    foreach (Entity item in entityGetdIntersect.Entities)
                    {
                        lisResult.Add((Guid)item[nnRelation.IntersectAttributeTo]);
                    }
                }
            }
            else
            {
                throw new Exception("Unsupport relation type " + relation.GetType());
            }
            return lisResult;

        }
    }
}
