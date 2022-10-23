using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    public abstract class Condition
    {
        protected Dictionary<ConditionOperator, @operator> m_queryVSfetchOperator = new Dictionary<ConditionOperator, @operator>()
        {
            { ConditionOperator.Equal, global::@operator.eq},
            //{ConditionOperator.NotEqual,global::@operator.neq}, //neq means not euqal to
            {ConditionOperator.NotEqual, global::@operator.ne}, //ne means Not Equals
            { ConditionOperator.GreaterThan , global::@operator.gt},
            { ConditionOperator.GreaterEqual,global::@operator.ge},
            { ConditionOperator.LessEqual,  global::@operator.le},
            { ConditionOperator.LessThan,   global::@operator.lt},
            { ConditionOperator.Like,           global::@operator.like},
            { ConditionOperator.NotLike,       global::@operator.notlike},
            { ConditionOperator.In,global::@operator.@in},
            {ConditionOperator.NotIn, global::@operator.notin},
            {ConditionOperator.Between,global::@operator.between},
            { ConditionOperator.NotBetween,global::@operator.notbetween},
            { ConditionOperator.Null,global::@operator.@null},
            { ConditionOperator.NotNull,global::@operator.notnull},
            { ConditionOperator.Yesterday,global::@operator.yesterday},
            { ConditionOperator.Today,global::@operator.today},
            { ConditionOperator.Tomorrow,global::@operator.tomorrow},
            { ConditionOperator.Last7Days,global::@operator.lastsevendays},
            { ConditionOperator.Next7Days,global::@operator.nextsevendays},
            { ConditionOperator.LastWeek,global::@operator.lastweek},
            { ConditionOperator.ThisWeek,global::@operator.thisweek},
            { ConditionOperator.NextWeek,global::@operator.nextweek},
            { ConditionOperator.LastMonth,global::@operator.lastmonth},
            { ConditionOperator.ThisMonth,global::@operator.thismonth},
            { ConditionOperator.NextMonth,global::@operator.nextmonth},
            { ConditionOperator.On,global::@operator.on},
            { ConditionOperator.OnOrBefore,global::@operator.onorbefore},
            { ConditionOperator.OnOrAfter,global::@operator.onorafter},
            { ConditionOperator.LastYear,global::@operator.lastyear},
            { ConditionOperator.ThisYear,global::@operator.thisyear},
            { ConditionOperator.NextYear,global::@operator.nextyear},
            { ConditionOperator.LastXHours,global::@operator.lastxhours},
            { ConditionOperator.NextXHours,global::@operator.nextxhours},
            { ConditionOperator.LastXDays,global::@operator.lastxdays},
            { ConditionOperator.NextXDays,global::@operator.nextxdays},
            { ConditionOperator.LastXWeeks,global::@operator.lastxweeks},
            { ConditionOperator.NextXWeeks,global::@operator.nextxweeks},
            { ConditionOperator.LastXMonths,global::@operator.lastxmonths},
            { ConditionOperator.NextXMonths,global::@operator.nextxmonths},
            { ConditionOperator.OlderThanXMonths,global::@operator.olderthanxmonths},
            { ConditionOperator.OlderThanXYears,global::@operator.olderthanxyears},
            { ConditionOperator.OlderThanXWeeks,global::@operator.olderthanxweeks},
            { ConditionOperator.OlderThanXDays,global::@operator.olderthanxdays},
            { ConditionOperator.OlderThanXHours,global::@operator.olderthanxhours},
            { ConditionOperator.OlderThanXMinutes,global::@operator.olderthanxminutes},
            { ConditionOperator.LastXYears,global::@operator.lastxyears},
            { ConditionOperator.NextXYears,global::@operator.nextxyears},
            { ConditionOperator.EqualUserId,global::@operator.equserid},
            { ConditionOperator.NotEqualUserId,global::@operator.neuserid},
            { ConditionOperator.EqualUserTeams,global::@operator.equserteams},
            { ConditionOperator.EqualUserOrUserTeams,global::@operator.equseroruserteams},
            { ConditionOperator.EqualUserOrUserHierarchy,global::@operator.equseroruserhierarchy},
            { ConditionOperator.EqualUserOrUserHierarchyAndTeams,global::@operator.equseroruserhierarchyandteams},
            { ConditionOperator.EqualBusinessId,global::@operator.eqbusinessid},
            { ConditionOperator.NotEqualBusinessId,global::@operator.nebusinessid},
            { ConditionOperator.EqualUserLanguage,global::@operator.equserlanguage},
            { ConditionOperator.ThisFiscalYear,global::@operator.thisfiscalyear},
            { ConditionOperator.ThisFiscalPeriod,global::@operator.thisfiscalperiod},
            { ConditionOperator.NextFiscalYear,global::@operator.nextfiscalyear},
            { ConditionOperator.NextFiscalPeriod,global::@operator.nextfiscalperiod},
            { ConditionOperator.LastFiscalYear,global::@operator.lastfiscalyear},
            { ConditionOperator.LastFiscalPeriod,global::@operator.lastfiscalperiod},
            { ConditionOperator.LastXFiscalYears,global::@operator.lastxfiscalyears},
            { ConditionOperator.LastXFiscalPeriods,global::@operator.lastxfiscalperiods},
            { ConditionOperator.NextXFiscalYears,global::@operator.nextxfiscalyears},
            { ConditionOperator.NextXFiscalPeriods,global::@operator.nextxfiscalperiods},
            { ConditionOperator.InFiscalYear,global::@operator.infiscalyear},
            { ConditionOperator.InFiscalPeriod,global::@operator.infiscalperiod},
            { ConditionOperator.InFiscalPeriodAndYear,global::@operator.infiscalperiodandyear},
            { ConditionOperator.InOrBeforeFiscalPeriodAndYear,global::@operator.inorbeforefiscalperiodandyear},
            { ConditionOperator.InOrAfterFiscalPeriodAndYear,global::@operator.inorafterfiscalperiodandyear},
            { ConditionOperator.BeginsWith,global::@operator.beginswith},
            { ConditionOperator.DoesNotBeginWith,global::@operator.notbeginwith},
            { ConditionOperator.EndsWith,global::@operator.endswith},
            { ConditionOperator.DoesNotEndWith,global::@operator.notendwith},
            { ConditionOperator.Under,global::@operator.under},
            { ConditionOperator.UnderOrEqual,global::@operator.eqorunder},
            { ConditionOperator.NotUnder,global::@operator.notunder},
            { ConditionOperator.Above,global::@operator.above},
            { ConditionOperator.AboveOrEqual,global::@operator.eqorabove}
        };

        public abstract Condition Clone();
        public virtual void AttachParent(FilterExpression parent)
        {

        }

        public virtual void AttachParent(filter parent)
        {

        }

        public virtual List<Relation> GetRelations()
        {
            return null;
        }

        public virtual FilterExpression GetQueryFilter()
        {
            return null;
        }

        public virtual filter GetFetchFilter()
        {
            return null;
        }
        public virtual void UpdateAlias(string alias)
        {
            return;
        }

        public virtual ConditionExpression GetQueryCondition()
        {
            return null;
        }

        public virtual condition GetFetchCondition()
        {
            return null;
        }

        public virtual void UpdateRelation(Relation relation)
        {

        }
        static public ConditionExpression CreateAlwaysFailedQueryCondition()
        {
            ConditionExpression cond = new ConditionExpression();
            cond.AttributeName = "createdon";
            cond.Operator = ConditionOperator.OnOrBefore;
            cond.Values.Add("1971-01-01");
            return cond;
        }

        static public condition CreateAlwaysFailedFetchCondition()
        {
            condition cond = new condition();
            cond.attribute = "createdon";
            cond.@operator = @operator.onorbefore;
            cond.value = "1971-01-01";
            return cond;
        }


        static public ConditionExpression CreateAlwaysSucceedQueryCondition()
        {
            ConditionExpression cond = new ConditionExpression();
            cond.AttributeName = "createdon";
            cond.Operator = ConditionOperator.OnOrAfter;
            cond.Values.Add("1971-01-01");
            return cond;
        }

        static public condition CreateAlwaysSucceedFetchCondition()
        {
            condition cond = new condition();
            cond.attribute = "createdon";
            cond.@operator = @operator.onorafter;
            cond.value = "1971-01-01";
            return cond;
        }


    }
}
