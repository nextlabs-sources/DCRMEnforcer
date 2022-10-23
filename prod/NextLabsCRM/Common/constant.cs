using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.Constant
{
    public class MessageName
    {
        public const string Retrieve = "Retrieve";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Retrievemultiple = "Retrievemultiple";
        public const string Delete = "Delete";
    }
    public class EntityName
    {
        public const string Savedqueryvisualization = "savedqueryvisualization";
        public const string Userqueryvisualization = "userqueryvisualization";
        public const string OpportunityEntity = "opportunity";
        public const string kbarticleEntity = "kbarticle";
        public const string productEntity = "product";
        public const string letterEntity = "letter";
        public const string Systemuser = "systemuser";
        public const string Incident = "incident";
        public const string NXLSPAction = "nxl_spaction";
    }
    public class AttributeKeyName
    {
        /// <summary>
        /// For Entity savedqueryvisualization at Retrive Post Event
        /// </summary>
        public const string Presentationdescription = "presentationdescription";
        public const string Datadescription = "datadescription";

        /// <summary>
        /// For Entity systemuser
        /// </summary>
        public const string SystemuserId = "systemuserid";
        public const string Fullname = "fullname";

        /// <summary>
        /// for all entity , it must have key name ownerid
        /// </summary>
        public const string OwnerId = "ownerid";

        public const string DomainName = "domainname";
        public const string InternalEmailaddress = "internalemailaddress";
        public const string processid = "processid";
        public const string traversedpath = "traversedpath";
        public const string statecode = "statecode";
        public const string kbarticletemplateid = "kbarticletemplateid";
        public const string iskit = "iskit";

        /// <summary>
        /// this variable for share variable
        /// </summary>
        public const string Nextlabs_ShareVariable_Key = "EEC92B00-E81F-4667-B9B2-34DDC2781120";
        public const string Nextlabs_ShareVariable_SPAction_Key = "bd45962b-5b04-4dc2-af2f-4602f4ec4078";

        public const string NXL_ISOwner = "nxl_isowner";
        public const string NXL_ISShared = "nxl_isshared";
        public const string NXL_SPAction_Id = "nxl_spactionid";
        public const string NXL_SPAction_Name = "nxl_name";
        public const string NXL_SPAction_Site = "nxl_site";
        public const string NXL_SPAction_FolderPath = "nxl_folderpath";
        public const string NXL_SPAction_Decision = "nxl_decision";
        public const string NXL_SPAction_ErrorMessage = "nxl_errormessage";
    }

    public class CacheControl
    {
        /// <summary>
        /// time span for cache expiry date
        /// </summary>
        public const int ValidInterval = 5;
        public const string DefaultExceptionMessage = "Access denied due to system error. Try again and contact the system administrator if the problem persists.";
        public const string DefaultDenyMessage = "Access denied, you are not authorized to perform this operation.";
    }

    public class Policy
    {
        public const string UserUnknown = "UnknownUser";
        public class Action
        {
            public const string Create = "CREATE";
            public const string Edit = "EDIT";
            public const string View = "VIEW";
            public const string Delete = "DELETE";
        }
        public class Obligation
        {
            public class Name
            {
                public const string DisplayViolationMessage = "dp_violation_message";
                public const string OwnerAlwayAllow = "owner_always_allow";
                public const string AppleSecurteFilter = "app_sec_filter";
            }
            public class Attribute
            {
                public const string Message = "message";
            }
        }
        
    }

    public class ParameterName
    {
        public const string ColumnSet = "ColumnSet";
        public const string BusinessEntityCollection = "BusinessEntityCollection";
        public const string BusinessEntity = "BusinessEntity";
        public const string Target = "Target";
        public const string Query = "Query";
    }

    public class DsiplayMessage
    {
        public const string PlugInRegisterStepNameFormat ="{0} of {1} on {2} event (Registed by NextLabs's Register Plugin)";
    }
    public class GeneralValue
    {
        public const string Yes = "Yes";
        public const string No = "No";
        public const string SystemUserFullname = "SYSTEM";
        public const int StepRank = 1;
        public const int DefaultLockTimeout = 1000 * 60;
    }
    public class TestConnectionParamter
    {
        public const string JPCAddress = "address";
        public const String OAuth2Address = "oAuthServerAddress";
        public const string ClientID = "clientID";
        public const string ClientSecret = "clientSecret";
        public const char SplitKeyAndKey = '&';
        public const char SplitKeyAandValue = '=';
    }
	public class ApplySecurityFilterOblOperator
    {
        public const string EqualsTo = "Equals To";
        public const string NotEqualTo = "Not Equal To";
        public const string GreaterThan = "Greater Than";
        public const string GreaterThanOrEqualsTo = "Greater Than or Equals To";
        public const string LessThan = "Less Than";
        public const string LessThanOrEqualsTo = "Less Than or Equals To";
        public const string IsNULL = "Is NULL";
        public const string IsNotNULL = "Is Not NULL";
        public const string OlderThanXdays = "Old Than X Days";
    }

    public class TestConnactionResultMessage
    {
        public const string ConnectToRemoteHostFaild = "Please check Policy Server Host";
        public const string ConnectToOauthHostFaild = "Please check Policy Server Host";
        public const string AuthenticatieFailed = "Please check client ID and Client Secret Key";
        public const string PolicyControllerStatusError = "System error: policy server cannot be connected";
        public const string Unknow = "Unknown system error";
        public static string Success = NextLabs.JavaPC.RestAPISDK.PCResult.OK.ToString();
    }

    public class ErrorCode
    {
        public const int EntityNotExist = 10000;
    }
}
