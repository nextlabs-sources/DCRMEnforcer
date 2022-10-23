using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    public class EntityOutJoin
    {
        //key is relationship name
        private Dictionary<string, Relation> m_relations = new Dictionary<string, Relation>();
        private Dictionary<string, List<Relation>> m_parentVSrelation = new Dictionary<string, List<Relation>>();
        public void AddRelation(Relation relation)
        {
            if (relation == null)
            {
                throw new ArgumentNullException("Relation must not be null");
            }

            if(!m_relations.ContainsKey(relation.Name))
            {
                m_relations[relation.Name] = relation;
            }
            else
            {
                Relation item = m_relations[relation.Name];
                if(!item.ConfictOrpanedAllow && (relation.OrphanedAllow != item.OrphanedAllow))
                {
                    item.ConfictOrpanedAllow = true;
                }
            }

            UpdateParentEntityRelationMap(relation);
        }

        public EntityOutJoin Merge(EntityOutJoin other)
        {
            EntityOutJoin target = new EntityOutJoin();
            foreach (Relation item in this.m_relations.Values)
            {
                target.AddRelation(item);
            }

            foreach (Relation item in other.m_relations.Values)
            {
                target.AddRelation(item);
            }
            return target;
        }

        private void UpdateParentEntityRelationMap(Relation item)
        {
            if (!m_parentVSrelation.ContainsKey(item.ParentEntity))
            {
                List<Relation> aliases = new List<Relation>();
                m_parentVSrelation[item.ParentEntity] = aliases;
            }
            m_parentVSrelation[item.ParentEntity].Add(item);
        }
        public List<FetchLinkEntityType> ToFetchLinkEntities(bool alias = true)
        {
            m_parentVSrelation.Clear();
            List<FetchLinkEntityType> linkEntities = new List<FetchLinkEntityType>();
            foreach (Relation item in m_relations.Values)
            {
                linkEntities.Add(item.ToFetchLinkEntity(alias));
                UpdateParentEntityRelationMap(item);
            }

            return linkEntities;
        }

        public List<LinkEntity> ToQueryLinkEntities(bool alias = true)
        {
            m_parentVSrelation.Clear();

            List<LinkEntity> linkEntities = new List<LinkEntity>();
            foreach (Relation item in m_relations.Values)
            {
                linkEntities.Add(item.ToQueryLinkEntity(alias));
                UpdateParentEntityRelationMap(item);
            }
            return linkEntities;
        }

        public List<Relation> GetRelationByParentName(string parentEntityName)
        {
            if(m_parentVSrelation.ContainsKey(parentEntityName))
            {
                return m_parentVSrelation[parentEntityName];
            }

            return null;
        }

        public Relation GetRelation(string relationshipName)
        {
            if (m_relations.ContainsKey(relationshipName))
            {
                return m_relations[relationshipName];
            }
            return null;
        }
    }
}
