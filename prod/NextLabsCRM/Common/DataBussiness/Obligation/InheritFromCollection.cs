using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    public class InheritFromCollection
    {
        class Item
        {
            public Item(List<InheritFrom> obligations)
            {
                m_obligations.AddRange(obligations);
            }
            private List<InheritFrom> m_obligations = new List<InheritFrom>();
            public List<InheritFrom> Obligations { get { return m_obligations; } }
        }

        List<Item> m_obligationLists = new List<Item>();
        public void Add(List<InheritFrom> obligations)
        {
            if (obligations != null)
            {
                m_obligationLists.Add(new Item(obligations));
            }
        }

        public void CreateEntityItems(out List<FetchLinkEntityType> linkentities, out filter obligationFilter)
        {

            obligationFilter = new filter();
            obligationFilter.type = filterType.and;
            obligationFilter.Items = new List<object>();

            EntityOutJoin totalJoin = new EntityOutJoin();
            foreach (Item item in m_obligationLists)
            {
                foreach (InheritFrom obligation in item.Obligations)
                {
                    totalJoin = obligation.OutJoin.Merge(totalJoin);
                }
            }

            foreach (Item item in m_obligationLists)
            {
                foreach (InheritFrom obligation in item.Obligations)
                {
                    obligation.OutJoin = totalJoin;
                    obligation.AddChildRelationFromVirtualCondition();
                }
            }

            //must be called before CreateTotalFetchFilter because of the alias
            linkentities = totalJoin.ToFetchLinkEntities();

            foreach (Item item in m_obligationLists)
            {
                filter subObligationFilter = new filter();
                subObligationFilter.type = filterType.or;
                subObligationFilter.Items = new List<object>();
                foreach (InheritFrom obligation in item.Obligations)
                {
                    subObligationFilter.Items.Add(obligation.CreateTotalFetchFilter());
                }
                obligationFilter.Items.Add(subObligationFilter);
            }
        }

        ////---------------------------------------------////
        public void CreateEntityItems(out List<LinkEntity> linkentities, out FilterExpression obligationFilter)
        { 

            EntityOutJoin totalJoin = new EntityOutJoin();
            foreach (Item item in m_obligationLists)
            {
                foreach (InheritFrom obligation in item.Obligations)
                {
                    totalJoin = obligation.OutJoin.Merge(totalJoin);
                }
            }

            foreach (Item item in m_obligationLists)
            {
                foreach (InheritFrom obligation in item.Obligations)
                {
                    obligation.OutJoin = totalJoin;
                    obligation.AddChildRelationFromVirtualCondition();
                }
            }

            //must be called before CreateTotalFetchFilter because of the alias
            linkentities = totalJoin.ToQueryLinkEntities();
            
            obligationFilter = new FilterExpression(LogicalOperator.And);
            foreach (Item item in m_obligationLists)
            {
                FilterExpression subObligationFilter = new FilterExpression(LogicalOperator.Or);
                foreach (InheritFrom obligation in item.Obligations)
                {
                    subObligationFilter.AddFilter(obligation.CreateTotalQueryFilter());
                }

                obligationFilter.AddFilter(subObligationFilter);
            }
        }
    }
}
