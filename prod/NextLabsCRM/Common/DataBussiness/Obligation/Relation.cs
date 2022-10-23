using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    public class Relation
    {
        protected string m_name;
        protected string m_currentEntity;
        protected string m_parentEntity;
        protected string m_currentAttribute;
        protected string m_parentAttribute;
        protected bool m_orphanedAllow;
        protected bool m_confilctOrphandAllow = false;
        protected string m_alias;
        protected List<Relation> m_childRelations = new List<Relation>();
        static private Random random = new Random((int)DateTime.Now.Ticks);
        public Relation(string name, string currentEntity, string parentEntity, string currentAttribute, string parentAttribute, bool orphanedAllow)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Relation name cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(currentEntity))
            {
                throw new ArgumentException("Entity name cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(parentEntity))
            {
                throw new ArgumentException("Parent Entity name cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(currentAttribute))
            {
                throw new ArgumentException("Attribute name cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(parentAttribute))
            {
                throw new ArgumentException("Parent Attribute name cannot be null or blank");
            }

            this.m_name = name;
            this.m_currentEntity = currentEntity;
            this.m_parentEntity = parentEntity;
            this.m_currentAttribute = currentAttribute;
            this.m_parentAttribute = parentAttribute;
            this.m_orphanedAllow = orphanedAllow;

        }

        protected string CreateAlias(string fromEntityName,string linkToEntityName)
        {
            string aliasPrefix = "nxl_";
            int ran = random.Next();
            return aliasPrefix + ran + "_" + fromEntityName+"_"+linkToEntityName;
        }
        public string Name { get { return m_name; } }
        public string CurrentEntity { get { return m_currentEntity; } }
        public string ParentEntity { get { return m_parentEntity; } }
        public string CurrentAttribute { get { return m_currentAttribute; } }
        public string ParentAttribute { get { return m_parentAttribute; } }
        public string ParentEntityAlias { get { return m_alias; } }
        public bool OrphanedAllow { get { return m_orphanedAllow; } }
        public bool ConfictOrpanedAllow { get { return m_confilctOrphandAllow; } set { m_confilctOrphandAllow = value; } }

        public void AddChildRelation(Relation child)
        {
            if(child == null)
            {
                return;
            }
            this.m_childRelations.Add(child);
        }

        public void AddChildRelations(List<Relation> children)
        {
            if (children == null)
            {
                return;
            }
            this.m_childRelations.AddRange(children);
        }
        protected static LinkEntity ToQueryLinkEntity(Relation currentRelation,bool isAlias)
        {
            if(currentRelation == null)
            {
                return null;
            }

            LinkEntity currentLinkEntity = currentRelation.ToSingleQueryLinkEntity(isAlias);
            if (currentRelation.m_childRelations.Count == 0)
            {
                return currentLinkEntity;
            }

            foreach(Relation childRelation in currentRelation.m_childRelations)
            {
                LinkEntity childLinkEntity = ToQueryLinkEntity(childRelation,isAlias);
                if (childLinkEntity != null)
                {
                    currentLinkEntity.LinkEntities.Add(childLinkEntity);
                }
            }
            return currentLinkEntity;
        }

        protected static FetchLinkEntityType ToFetchLinkEntity(Relation currentRelation, bool isAlias)
        {
            if (currentRelation == null)
            {
                return null;
            }

            FetchLinkEntityType currentLinkEntity = currentRelation.ToSingleFetchLinkEntity(isAlias);
            if (currentRelation.m_childRelations.Count == 0)
            {
                return currentLinkEntity;
            }

            foreach (Relation childRelation in currentRelation.m_childRelations)
            {
                FetchLinkEntityType childLinkEntity = ToFetchLinkEntity(childRelation, isAlias);
                if (childLinkEntity != null)
                {
                    if (currentLinkEntity.Items == null)
                    {
                        currentLinkEntity.Items = new List<object>();
                    }
                    currentLinkEntity.Items.Add(childLinkEntity);
                }
            }
            return currentLinkEntity;
        }

        public FetchLinkEntityType ToFetchLinkEntity(bool alias = true)
        {
            return ToFetchLinkEntity(this, alias);
        }
        protected virtual FetchLinkEntityType ToSingleFetchLinkEntity(bool alias = true)
        {
            FetchLinkEntityType linker = new FetchLinkEntityType();
            linker.name = m_parentEntity;
            linker.from = m_parentAttribute;
            linker.to = m_currentAttribute;
            linker.linktype = "outer";

            if(alias)
            { 
                linker.alias = CreateAlias(CurrentEntity,m_parentEntity);
                m_alias = linker.alias;
            }
            return linker;
        }

        public LinkEntity ToQueryLinkEntity(bool alias = true)
        {
            return ToQueryLinkEntity(this, alias);
        }
        protected virtual LinkEntity ToSingleQueryLinkEntity(bool alias = true)
        {
            LinkEntity linker = new LinkEntity(m_currentEntity,
                                               m_parentEntity,
                                               m_currentAttribute,
                                               m_parentAttribute,
                                               JoinOperator.LeftOuter);

            if (alias)
            { 
                linker.EntityAlias = CreateAlias(CurrentEntity,m_parentEntity);
                m_alias = linker.EntityAlias; 
            }
            return linker;
        }
    }

    public class N1Relation : Relation
    {
        public N1Relation(string name, string currentEntity, string parentEntity, string currentAttribute, string parentAttribute, bool orphanedAllow = false)
            : base(name, currentEntity, parentEntity, currentAttribute, parentAttribute, orphanedAllow)
        {

        }

        public N1Relation(string entityName, DataModel.N1RelationShipItem item, bool orphanedAllow = false):
            base(item.Name, entityName, item.PrimaryEntityLogicName, item.Field, item.PrimaryEntityFieldLogicName, orphanedAllow)
        {
            if(item == null)
            {
                throw new ArgumentNullException("RelationshipItem cannot be null.");
            }
        }
    }

    public class NNRelation : Relation
    {
        private string m_intersectEntity;
        private string m_intersectAttributeFrom;
        private string m_intersectAttributeTo;

        public NNRelation(string name, string currentEntity, string parentEntity, string currentAttribute, string parentAttribute,
            string intersectEntity, string intersectAttributeFrom, string intersectAttributeTo, bool orphanedAllow = false)
            : base(name, currentEntity, parentEntity, currentAttribute, parentAttribute, orphanedAllow)
        {
            if (string.IsNullOrWhiteSpace(intersectEntity))
            {
                throw new ArgumentException("Intersect Entity name cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(intersectAttributeFrom))
            {
                throw new ArgumentException("Intersect Attribute(from) cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(intersectAttributeTo))
            {
                throw new ArgumentException("Intersect Attribute(to) cannot be null or blank");
            }

            m_intersectEntity = intersectEntity;
            m_intersectAttributeFrom = intersectAttributeFrom;
            m_intersectAttributeTo = intersectAttributeTo;
        }

        public NNRelation(string entityName, DataModel.NNRelationShipItem item, bool orphanedAllow = false):
            base(item.Name, entityName, item.PrimaryEntityLogicName, item.Field, item.PrimaryEntityFieldLogicName, orphanedAllow)
        {
            if (string.IsNullOrWhiteSpace(item.RelationshipEntityLogicName))
            {
                throw new ArgumentException("Intersect Entity name cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(item.Field))
            {
                throw new ArgumentException("Intersect Attribute(from) cannot be null or blank");
            }

            if (string.IsNullOrWhiteSpace(item.PrimaryEntityFieldLogicName))
            {
                throw new ArgumentException("Intersect Attribute(to) cannot be null or blank");
            }

            m_intersectEntity = item.RelationshipEntityLogicName;
            m_intersectAttributeFrom = item.Field;
            m_intersectAttributeTo = item.PrimaryEntityFieldLogicName;
        }
        public string IntersectEntity { get { return m_intersectEntity; } }
        public string IntersectAttributeFrom { get { return m_intersectAttributeFrom; } }
        public string IntersectAttributeTo { get { return m_intersectAttributeTo; } }

        protected override FetchLinkEntityType ToSingleFetchLinkEntity(bool alias = true)
        {
            FetchLinkEntityType linker = new FetchLinkEntityType();
            linker.name = m_intersectEntity;
            linker.from = m_intersectAttributeFrom;
            linker.to = m_currentAttribute;
            linker.linktype = "outer";
            linker.Items = new List<object>();

            FetchLinkEntityType sublinker = new FetchLinkEntityType();
            sublinker.name = m_parentEntity;
            sublinker.from = m_parentAttribute;
            sublinker.to = m_intersectAttributeTo;
            sublinker.linktype = "outer";

            if (alias)
            {
                sublinker.alias = CreateAlias(CurrentEntity,ParentAttribute);
                m_alias = sublinker.alias;
            }
            linker.Items.Add(sublinker);
            return linker;
        }
        protected override LinkEntity ToSingleQueryLinkEntity(bool alias = true)
        {
            LinkEntity linker = new LinkEntity(m_currentEntity,
                                                                m_intersectEntity,
                                                                m_currentAttribute,
                                                                m_intersectAttributeFrom,
                                                                JoinOperator.LeftOuter);

            LinkEntity sublinker = new LinkEntity(m_intersectEntity,
                                                                    m_parentEntity,
                                                                    m_intersectAttributeTo,
                                                                    m_parentAttribute,
                                                                    JoinOperator.LeftOuter);

            if (alias)
            {
                sublinker.EntityAlias = CreateAlias(CurrentEntity,m_parentEntity);
                m_alias = sublinker.EntityAlias;
            }
            linker.LinkEntities.Add(sublinker);
            
            return linker;
        }
    }
}
