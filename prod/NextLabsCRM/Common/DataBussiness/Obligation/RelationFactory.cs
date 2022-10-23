using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    class RelationFactory
    {
        static public Relation CreateRelation(MemoryCache<DataModel.N1Relationship> N1Cache, string entityName,string relationshipName,bool orphanedAllow)
        {
            if(N1Cache == null || string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(relationshipName))
            {
                return null;
            }
            DataModel.N1Relationship relations = N1Cache.Lookup(entityName, x => null);
            if(relations == null)
            {
                return null;
            }

            DataModel.N1RelationShipItem item = relations.FindRelationshipByName(relationshipName);
            if(item == null)
            {
                return null;
            }

            return new N1Relation(entityName, item, orphanedAllow);
        }

        static public Relation CreateRelation(MemoryCache<DataModel.N1Relationship> N1Cache,MemoryCache<DataModel.NNRelationship> NNCache, string entityName, string relationshipName, bool orphanedAllow)
        {
            Common.DataBussiness.Obligation.Relation relation = Common.DataBussiness.Obligation.RelationFactory.CreateRelation(N1Cache, entityName, relationshipName, orphanedAllow);
            if (relation == null)
            {
                relation = Common.DataBussiness.Obligation.RelationFactory.CreateRelation(NNCache, entityName, relationshipName, orphanedAllow);
            }
            if(relation!=null)
            {
                return relation;
            }
            else
            {
                throw new Exceptions.InvailRelationshipException(entityName, relationshipName, N1Cache, NNCache);
            }
        }

        static public Relation CreateRelation(MemoryCache<DataModel.NNRelationship> NNCache, string entityName, string relationshipName, bool orphanedAllow)
        {
            if (NNCache == null || string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(relationshipName))
            {
                return null;
            }

            DataModel.NNRelationship relations = NNCache.Lookup(entityName, x => null);
            if (relations == null)
            {
                return null;
            }

            DataModel.NNRelationShipItem item = relations.FindRelationshipByName(relationshipName);
            if (item == null)
            {
                return null;
            }

            return new NNRelation(entityName, item, orphanedAllow);
        }
    }
}
