using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.Enums
{
    public enum emPolicyResult
    {
        em_Allow,
        em_Deny
    }

    public enum emCRMUserAction
    {
        emUNKNOWN_ACTION,
        em_LIST_ACTION,
        em_CREATE_ACTION,
        em_DELETE_ACTION,
        em_EDIT_ACTION,
        em_CLICK_RECORD_ACTION,
    }
    //10 (pre-validation), 20 (pre-operation), 40 (post-operation), and 50 (post-operation,deprecated).
    public enum emStepStage
    {
        Pre_validation = 10,
        Pre_operation = 20,
        post_operation = 40,
        post_operation_deprecated = 50
    }
    public enum emStepMode
    {
        em_Synchronous = 0
    }
    public enum emStepDeployment
    {
        em_Server = 0
    }
    public enum emCRMDATAMATCHOBLDATA
    {
        em_INIT,
        em_DOT_MATCH,
        em_MATCH,
    }
    //default log level is error
    public enum LogLevel
    {
        Error,
        Debug,
        Warning,
        Information,
        None
    }
    public enum Decision
    {
        Deny,
        Allow,
        Unknow
    }
}
