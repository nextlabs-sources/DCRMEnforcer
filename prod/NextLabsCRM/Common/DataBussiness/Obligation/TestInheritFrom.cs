using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    //public class TestInheritFrom
    //{

    //    static private ConditionExpression CreateCompetitorOblItem1()
    //    {
    //        ConditionExpression item = new ConditionExpression();
    //        item.AttributeName = "name";
    //        item.Operator = Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal;
    //        item.Values.Add("will_competitor");

    //        return item;
    //    }

    //    static private ConditionExpression CreateCompetitorOblItem2()
    //    {
    //        ConditionExpression item = new ConditionExpression();
    //        item.AttributeName = "address1_composite";
    //        item.Operator = Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal;
    //        item.Values.Add("WestLake #1 street");

    //        return item;
    //    }

    //    static private ConditionExpression CreateAccountOblItem1()
    //    {
    //        ConditionExpression item = new ConditionExpression();
    //        item.AttributeName = "name";
    //        item.Operator = Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal;
    //        item.Values.Add("jin_account");

    //        return item;
    //    }

    //    static private ConditionExpression CreateErrorAccountOblItem1()
    //    {
    //        ConditionExpression item = new ConditionExpression();
    //        item.AttributeName = "name";
    //        item.Operator = Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal;
    //        item.Values.Add("jin_account_error");

    //        return item;
    //    }

    //    static private ConditionExpression CreateAccountOblItem2()
    //    {
    //        ConditionExpression item = new ConditionExpression();
    //        item.AttributeName = "emailaddress1";
    //        item.Operator = Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal;
    //        item.Values.Add("will.wei@nextlabs.com");

    //        return item;
    //    }
    //    static public void TestOppCompetitorAllowCase()
    //    {

    //    }

    //    static public void TestOppAccountAllowCase()
    //    {

    //    }

    //    static private QueryExpression AppendItemToQe(QueryExpression qe,FilterExpression filter,List<LinkEntity> links)
    //    {
    //        qe.LinkEntities.AddRange(links);
    //        if (qe.Criteria == null)
    //        {
    //            qe.Criteria = new FilterExpression(LogicalOperator.And);
    //        }

    //        if (qe.Criteria.FilterOperator == LogicalOperator.Or)
    //        {
    //            FilterExpression totalFilter = new FilterExpression(LogicalOperator.And);
    //            totalFilter.AddFilter(qe.Criteria);
    //            totalFilter.AddFilter(filter);
    //            qe.Criteria = totalFilter;
    //        }
    //        else
    //        {
    //            qe.Criteria.AddFilter(filter);
    //        }
    //        return qe;
    //    }
    //    static public void TestOppAccountAndCompetitorAllowCase(QueryExpression qe, bool orphanedChildAllowed)
    //    {
    //        InheritPolicyFrom obl = new InheritPolicyFrom();
    //        N1Relation relation1 = new N1Relation("opportunity_parent_account",
    //            "opportunity",
    //            "account",
    //            "parentaccountid",
    //            "accountid",
    //            orphanedChildAllowed
    //            );
    //        obl.AddRelation(relation1);

    //        NNRelation relation2 = new NNRelation("opportunitycompetitors_association",
    //            "opportunity",
    //            "competitor",
    //            "opportunityid",
    //            "competitorid",
    //            "opportunitycompetitors",
    //            "opportunityid",
    //            "competitorid",
    //            orphanedChildAllowed
    //            );
    //        obl.AddRelation(relation2);


    //        ApplySecurityFilter filter1 = new ApplySecurityFilter();
    //        filter1.EntityName = "competitor";
    //        filter1.AddCondition(new CommonCondition(CreateCompetitorOblItem1()));
    //        filter1.AddCondition(new CommonCondition(CreateCompetitorOblItem2()));

    //        ApplySecurityFilter filter2 = new ApplySecurityFilter();
    //        filter2.EntityName = "account";
    //        filter2.AddCondition(new CommonCondition(CreateAccountOblItem1()));
    //        filter2.AddCondition(new CommonCondition(CreateAccountOblItem2()));

    //        obl.AddParentSecurityFilter(filter1);
    //        obl.AddParentSecurityFilter(filter2);

    //        InheritFromCollection collection = new InheritFromCollection();
    //        collection.Add(new List<InheritFrom>() { obl });

    //        List<LinkEntity> entities = null;
    //        FilterExpression filter = null;
    //        collection.CreateEntityItems(out entities, out filter);

    //        AppendItemToQe(qe, filter, entities);
    //    }

    //    static public void TestOppAccountOrCompetitorAllowCase(QueryExpression qe, bool orphanedChildAllowed)
    //    {
    //        InheritPolicyFrom obl1 = new InheritPolicyFrom();
    //        N1Relation relation1 = new N1Relation("opportunity_parent_account",
    //            "opportunity",
    //            "account",
    //            "parentaccountid",
    //            "accountid",
    //            orphanedChildAllowed
    //            );
    //        obl1.AddRelation(relation1);

    //        ApplySecurityFilter filter1 = new ApplySecurityFilter();
    //        filter1.EntityName = "account";
    //        filter1.AddCondition(new CommonCondition(CreateAccountOblItem1()));
    //        filter1.AddCondition(new CommonCondition(CreateAccountOblItem2()));

    //        obl1.AddParentSecurityFilter(filter1);

    //        InheritPolicyFrom obl2 = new InheritPolicyFrom();
    //        NNRelation relation2 = new NNRelation("opportunitycompetitors_association",
    //            "opportunity",
    //            "competitor",
    //            "opportunityid",
    //            "competitorid",
    //            "opportunitycompetitors",
    //            "opportunityid",
    //            "competitorid",
    //            orphanedChildAllowed
    //            );
    //        obl2.AddRelation(relation2);


    //        ApplySecurityFilter filter2 = new ApplySecurityFilter();
    //        filter2.EntityName = "competitor";
    //        filter2.AddCondition(new CommonCondition(CreateCompetitorOblItem1()));
    //        filter2.AddCondition(new CommonCondition(CreateCompetitorOblItem2()));
    //        obl2.AddParentSecurityFilter(filter2);

    //        InheritFromCollection collection = new InheritFromCollection();
    //        collection.Add(new List<InheritFrom>() { obl1, obl2 });

    //        List<LinkEntity> entities = null;
    //        FilterExpression filter = null;
    //        collection.CreateEntityItems(out entities, out filter);

    //        AppendItemToQe(qe, filter, entities);
    //    }

    //    static public void TestOppAccountDenyOrCompetitorAllowCase(QueryExpression qe, bool orphanedChildAllowed)
    //    {
    //        InheritPolicyFrom obl1 = new InheritPolicyFrom();
    //        N1Relation relation1 = new N1Relation("opportunity_parent_account",
    //            "opportunity",
    //            "account",
    //            "parentaccountid",
    //            "accountid",
    //            orphanedChildAllowed
    //            );
    //        obl1.AddRelation(relation1);

    //        ApplySecurityFilter filter1 = new ApplySecurityFilter();
    //        filter1.EntityName = "account";
    //        filter1.AddCondition(new CommonCondition(CreateErrorAccountOblItem1()));
    //        filter1.AddCondition(new CommonCondition(CreateAccountOblItem2()));

    //        obl1.AddParentSecurityFilter(filter1);

    //        InheritPolicyFrom obl2 = new InheritPolicyFrom();
    //        NNRelation relation2 = new NNRelation("opportunitycompetitors_association",
    //            "opportunity",
    //            "competitor",
    //            "opportunityid",
    //            "competitorid",
    //            "opportunitycompetitors",
    //            "opportunityid",
    //            "competitorid",
    //            orphanedChildAllowed
    //            );
    //        obl2.AddRelation(relation2);


    //        ApplySecurityFilter filter2 = new ApplySecurityFilter();
    //        filter2.EntityName = "competitor";
    //        filter2.AddCondition(new CommonCondition(CreateCompetitorOblItem1()));
    //        filter2.AddCondition(new CommonCondition(CreateCompetitorOblItem2()));
    //        obl2.AddParentSecurityFilter(filter2);

    //        InheritFromCollection collection = new InheritFromCollection();
    //        collection.Add(new List<InheritFrom>() { obl1, obl2 });

    //        List<LinkEntity> entities = null;
    //        FilterExpression filter = null;
    //        collection.CreateEntityItems(out entities, out filter);

    //        AppendItemToQe(qe, filter, entities);
    //    }


    //    static public void TestOppAccountDenyAndCompetitorAllowCase(QueryExpression qe, bool orphanedChildAllowed)
    //    {
    //        InheritPolicyFrom obl = new InheritPolicyFrom();
    //        N1Relation relation1 = new N1Relation("opportunity_parent_account",
    //            "opportunity",
    //            "account",
    //            "parentaccountid",
    //            "accountid",
    //            orphanedChildAllowed
    //            );
    //        obl.AddRelation(relation1);

    //        NNRelation relation2 = new NNRelation("opportunitycompetitors_association",
    //            "opportunity",
    //            "competitor",
    //            "opportunityid",
    //            "competitorid",
    //            "opportunitycompetitors",
    //            "opportunityid",
    //            "competitorid",
    //            orphanedChildAllowed
    //            );
    //        obl.AddRelation(relation2);


    //        ApplySecurityFilter filter1 = new ApplySecurityFilter();
    //        filter1.EntityName = "competitor";
    //        filter1.AddCondition(new CommonCondition(CreateCompetitorOblItem1()));
    //        filter1.AddCondition(new CommonCondition(CreateCompetitorOblItem2()));

    //        ApplySecurityFilter filter2 = new ApplySecurityFilter();
    //        filter2.EntityName = "account";
    //        filter2.AddCondition(new CommonCondition(CreateErrorAccountOblItem1()));
    //        filter2.AddCondition(new CommonCondition(CreateAccountOblItem2()));

    //        obl.AddParentSecurityFilter(filter1);
    //        obl.AddParentSecurityFilter(filter2);

    //        InheritFromCollection collection = new InheritFromCollection();
    //        collection.Add(new List<InheritFrom>() { obl });

    //        List<LinkEntity> entities = null;
    //        FilterExpression filter = null;
    //        collection.CreateEntityItems(out entities, out filter);

    //        AppendItemToQe(qe, filter, entities);
    //    }
    //}
}
