using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using NextLabs.CRMEnforcer.Common.Constant;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;

namespace NextLabs.CRMEnforcer.Exceptions
{
    [Serializable]
    public class InvalidFilterException : ApplicationException
    {
        public InvalidFilterException() { }
        public InvalidFilterException(string message) : base(message) { }
        public InvalidFilterException(string message, Exception inner): base(message, inner) { }

        public InvalidFilterException(string strEntityName,string strField,ConditionOperator operation,string strValue):base(string.Format("This filter cannot work {0},{1},{2},{3}", strEntityName, strField, operation.ToString(), strValue))
        {

        }
    }
    [Serializable]
    public class InvalidOperationException:ApplicationException
    {
        public InvalidOperationException(Type datetype, ConditionOperator operation) : base(string.Format(" Date Type {0} don't support operate {1}", datetype.ToString(), operation.ToString())) { }
    }
    [Serializable]
    public class InvalidObligationException : ApplicationException
    {
        public InvalidObligationException() { }
        public InvalidObligationException(string message)
            : base(message) { }
        public InvalidObligationException(CEObligation obligation):base("Cannot anaysis this obligation, detail:" + obligation.ToString())
        {

        }
        public InvalidObligationException(string message, Exception inner)
            : base(message, inner) { }
    }
    [Serializable]
    public class InvalidCacheException:ApplicationException
    {
        public InvalidCacheException() { }
        public InvalidCacheException(string strEntityName)
            : base(string.Format("Cannot find entity {0} struct on memory cache", strEntityName)) { }
        public InvalidCacheException(string message, Exception inner)
            : base(message, inner) { }
        public InvalidCacheException(SecureEntity secureEntityCurrent,string strMessage) : base("Cache detail:"+secureEntityCurrent.ToString()+" Error message"+strMessage) { }

    }
    [Serializable]
    public class InvailRelationshipException:ApplicationException
    {
        public InvailRelationshipException(string strEntityName, string strRelationName, MemoryCache<Common.DataModel.N1Relationship> N1Cache, MemoryCache<Common.DataModel.NNRelationship> NNCache):base(string.Format("Cannot find entity name {3} relationship {0} on Cache, detail N:1 relationship {1},N:N relationship {2}", strRelationName, N1Cache.ToString(), NNCache.ToString(), strEntityName)) { }
    }
    [Serializable]
    public class InvalidMessageException:ApplicationException
    {
        public InvalidMessageException() : base() { }
        public InvalidMessageException(string message) : base(message){}
        public InvalidMessageException(string message, Exception inner) : base(message, inner) { }
        public InvalidMessageException(string strTypeName, string strMethodName, string strMessageName) : base(string.Format("This type {0}.{1} don't support message {2}", strTypeName ,strMethodName, strMessageName)) { }
    }
    [Serializable]
    public class InvalidDataTypeException:ApplicationException
    {
        public InvalidDataTypeException() : base() { }
        public InvalidDataTypeException(string strType,string strMethod,string strDataType) : base(string.Format("This type {0}.{1} don't support datatype {2}", strType, strMethod, strDataType)) { }
    }
    [Serializable]
    public class RestPDPException:ApplicationException
    {
        public RestPDPException(string strTypeName, string strMethodName, NextLabs.JavaPC.RestAPISDK.PCResult pcResult, NextLabs.JavaPC.RestAPISDK.JavaPC restPDP) : base(string.Format("Rest PDP return {0} on type {1} method {2}, PDP detail {3}", pcResult, strTypeName, strMethodName, restPDP.ToString())) { }
    }
    [Serializable]
    public class ParameterNotFoundException: ApplicationException
    {
        public ParameterNotFoundException(string strParamerName,string strMessage,string strEntityName):
            base(string.Format("Cannot found parameter {0} on {1} of {2}", strParamerName, strMessage, strEntityName)) { }
    }
    [Serializable]
    public class EnforceNotExcuted:ApplicationException
    {
        public EnforceNotExcuted() : base("Cannot get enforce result, please run 'DoEnforce' method") { }
    }

    [Serializable]
    public class InvalidUserException : ApplicationException
    {
        public InvalidUserException(Guid userId, string strReason) : base(string.Format("User {0} is invalid, because:{1}", userId, strReason)) { }
    }

    [Serializable]
    public class EntityNotExistException : Exception
    {
        public EntityNotExistException(string strEntityName, string strReason) : 
            base(string.Format("[{0}]The entity {1} does not exist, because:{2}", ErrorCode.EntityNotExist, strEntityName, strReason)) { }
    }
}
