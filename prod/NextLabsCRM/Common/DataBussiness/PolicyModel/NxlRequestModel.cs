using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.PolicyModel
{
    public class NxlRequestModel
    {

        [DataContract]
        public class AddPolicyModel
        {

            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string description { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string status { get; set; }

            [DataMember]
            public List<TagsItem> tags;

            [DataMember]
            public List<AttributesItem> attributes;

            [DataMember]
            public List<ActionsItem> actions;

            [DataMember]
            public List<ObligationsItem> obligations;

            [DataMember]
            public string version { get; set; }

            public AddPolicyModel() {
                tags = new List<TagsItem>();
                attributes = new List<AttributesItem>();
                actions = new List<ActionsItem>();
                obligations = new List<ObligationsItem>();
            }

            public AddPolicyModel(PolicyModelDetail.Data proto) {
                id = proto.id;
                name = proto.name;
                shortName = proto.shortName;
                description = proto.description;
                type = proto.type;
                status = proto.status;
                version = proto.version;
                tags = new List<TagsItem>();
                //foreach (PolicyModelDetail.TagsItem tag in proto.tags)
                //{
                //    tags.Add(new TagsItem(tag));
                //}
                attributes = new List<AttributesItem>();
                foreach (PolicyModelDetail.AttributesItem attr in proto.attributes)
                {
                    attributes.Add(new AttributesItem(attr));
                }
                actions = new List<ActionsItem>();
                foreach (PolicyModelDetail.ActionsItem action in proto.actions)
                {
                    actions.Add(new ActionsItem(action));
                }
                obligations = new List<ObligationsItem>();
                foreach (PolicyModelDetail.ObligationsItem ob in proto.obligations)
                {
                    obligations.Add(new ObligationsItem(ob));
                }
            }
            public AddPolicyModel(LocalPolicyModel.PolicyModelsItem local, Dictionary<string, Dictionary<string, int>> dicOp)
            {
                id = null;
                name = local.name;
                shortName = local.shortName;
                description = local.description;
                type = local.type;
                status = local.status;
                version = null;
                tags = new List<TagsItem>();
                //foreach (LocalPolicyModel.TagsItem tag in local.tags)
                //{
                //    tags.Add(new TagsItem(tag));
                //}
                attributes = new List<AttributesItem>();
                int attrorder = 0;
                foreach (LocalPolicyModel.AttributesItem attr in local.attributes)
                {
                    attributes.Add(new AttributesItem(attr, attrorder++, dicOp));
                }
                actions = new List<ActionsItem>();
                int actorder = 0;
                foreach (LocalPolicyModel.ActionsItem action in local.actions)
                {
                    actions.Add(new ActionsItem(action, actorder++));
                }
                obligations = new List<ObligationsItem>();
                int oborder = 0;
                foreach (LocalPolicyModel.ObligationsItem ob in local.obligations)
                {
                    obligations.Add(new ObligationsItem(ob, oborder++));
                }
            }
        }

        [DataContract]
        public class ObligationsItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string runAt { get; set; }

            [DataMember]
            public List<ParametersItem> parameters = new List<ParametersItem>();

            [DataMember]
            public int sortOrder { get; set; }

            public ObligationsItem()
            {

            }

            public ObligationsItem(PolicyModelDetail.ObligationsItem ob)
            {
                id = null;
                name = ob.name;
                shortName = ob.shortName;
                runAt = ob.runAt;
                sortOrder = ob.sortOrder;
                parameters = new List<ParametersItem>();
                foreach (PolicyModelDetail.ParametersItem param in ob.parameters)
                {
                    parameters.Add(new ParametersItem(param));
                }
            }

            public ObligationsItem(LocalPolicyModel.ObligationsItem ob, int order)
            {
                id = null;
                name = ob.name;
                shortName = ob.shortName;
                runAt = ob.runAt;
                sortOrder = order;
                parameters = new List<ParametersItem>();
                int porder = 0;
                foreach (LocalPolicyModel.ParametersItem param in ob.parameters)
                {
                    parameters.Add(new ParametersItem(param, porder++));
                }

            }
        }

        [DataContract]
        public class ParametersItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string defaultValue { get; set; }

            [DataMember]
            public string value { get; set; }

            [DataMember]
            public string listValues { get; set; }

            [DataMember]
            public bool hidden { get; set; }

            [DataMember]
            public bool editable { get; set; }

            [DataMember]
            public bool mandatory { get; set; }

            [DataMember]
            public int sortOrder { get; set; }

            [DataMember]
            public string shortName { get; set; }

            public ParametersItem()
            {

            }

            public ParametersItem(PolicyModelDetail.ParametersItem param) {
                id = null;// param.id;
                name = param.name;
                type = param.type;
                defaultValue = param.defaultValue;
                value = "";
                listValues = param.listValues;
                hidden = param.hidden;
                editable = param.editable;
                mandatory = param.mandatory;
                shortName = param.shortName;
                sortOrder = param.sortOrder;
            }

            public ParametersItem(LocalPolicyModel.ParametersItem param, int order)
            {
                id = null;
                name = param.name;
                type = param.type;
                defaultValue = param.defaultValue;
                value = "";
                listValues = param.listValues;
                hidden = param.hidden;
                editable = param.editable;
                mandatory = param.mandatory;
                shortName = param.shortName;
                sortOrder = order;
            }
        }

        [DataContract]
        public class ActionsItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public int sortOrder { get; set; }

            public ActionsItem()
            {

            }

            public ActionsItem(PolicyModelDetail.ActionsItem action)
            {
                id = action.id;
                name = action.name;
                shortName = action.shortName;
                sortOrder = action.sortOrder;
            }

            public ActionsItem(LocalPolicyModel.ActionsItem action, int order)
            {
                id = null;
                name = action.name;
                shortName = action.shortName;
                sortOrder = order;
            }
        }

        [DataContract]
        public class AttributesItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string dataType { get; set; }

            [DataMember]
            public List<OperatorConfigsItem> operatorConfigs { get; set; }

            [DataMember]
            public string regExPattern { get; set; }

            [DataMember]
            public int sortOrder { get; set; }

            public AttributesItem()
            {

            }

            public AttributesItem(PolicyModelDetail.AttributesItem attr)
            {
                id = null;
                name = attr.name;
                shortName = attr.shortName;
                dataType = attr.dataType;
                regExPattern = attr.regExPattern;
                sortOrder = attr.sortOrder;
                operatorConfigs = new List<OperatorConfigsItem>();
                foreach (PolicyModelDetail.OperatorConfigsItem op in attr.operatorConfigs) {
                    operatorConfigs.Add(new OperatorConfigsItem(op));
                }
            }

            public AttributesItem(LocalPolicyModel.AttributesItem attr, int order, Dictionary<string, Dictionary<string, int>> dicOp)
            {
                id = null;// attr.id;
                name = attr.name;
                shortName = attr.shortName;
                dataType = attr.dataType;
                regExPattern = null;
                sortOrder = order;
                operatorConfigs = new List<OperatorConfigsItem>();
                foreach (LocalPolicyModel.OperatorConfigsItem op in attr.operatorConfigs)
                {
                    operatorConfigs.Add(new OperatorConfigsItem(op).OpIdConvert(dicOp));
                }
            }
        }

        [DataContract]
        public class OperatorConfigsItem
        {
            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string key { get; set; }

            [DataMember]
            public string label { get; set; }

            [DataMember]
            public string dataType { get; set; }

            public OperatorConfigsItem()
            {

            }

            public OperatorConfigsItem(PolicyModelDetail.OperatorConfigsItem op) {
                id = op.id;
                key = op.key;
                label = op.label;
                dataType = op.dataType;
            }

            public OperatorConfigsItem(LocalPolicyModel.OperatorConfigsItem op)
            {
                id = op.id;
                key = op.key;
                label = op.label;
                dataType = op.dataType;
            }
            //translator Local Op to Req Op
            public OperatorConfigsItem OpIdConvert(Dictionary<string, Dictionary<string, int>> dicOp) {
                if (dicOp.ContainsKey(dataType))
                {
                    if (dicOp[dataType].ContainsKey(key))
                    {
                        id = dicOp[dataType][key];
                    }
                }
                return this;
            }
        }

        [DataContract]
        public class TagsItem
        {
            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string key { get; set; }

            [DataMember]
            public string label { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string status { get; set; }

            public TagsItem()
            {

            }

            public TagsItem(TagsRepModel.DataItem tag)
            {
                id = tag.id;
                key = tag.key;
                label = tag.label;
                type = tag.type;
                status = tag.status;
            }

            public TagsItem(PolicyModelDetail.TagsItem tag)
            {
                id = tag.id;
                key = tag.key;
                label = tag.label;
                type = tag.type;
                status = tag.status;
            }

            public TagsItem(LocalPolicyModel.TagsItem tag)
            {
                id = tag.id;
                key = tag.key;
                label = tag.label;
                type = tag.type;
                status = tag.status;
            }

            public TagsItem(int id1, string key1, string label1, string type1, string status1)
            {
                id = id1;
                key = key1;
                label = label1;
                type = type1;
                status = status1;
            }
        }

        [DataContract]
        public class ConditionsItem
        {
            [DataMember]
            public string attribute { get; set; }

            [DataMember(Name = "operator")]
            public string _operator { get; set; }

            [DataMember]
            public string value { get; set; }

        }

        [DataContract]
        public class PolicyModel
        {
            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }           

        }

        [DataContract]
        public class AddComponent
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string description { get; set; }

            [DataMember]
            public List<TagsItem> tags { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public List<ConditionsItem> conditions { get; set; }

            [DataMember]
            public List<string> actions { get; set; }

            [DataMember]
            public List<string> subComponents { get; set; }

            [DataMember]
            public string version { get; set; }

            [DataMember]
            public PolicyModel policyModel { get; set; }

            [DataMember]
            public string status { get; set; }

            public AddComponent(List<NxlRequestModel.TagsItem> componentTags)
            {
                id = null;
                description = null;
                type = "ACTION";
                conditions = new List<ConditionsItem>();
                subComponents = new List<string>();
                version = null;
                status = "DRAFT";
                tags = new List<TagsItem>();
                foreach (TagsItem tag in componentTags)
                {
                    //tags.Add(new TagsItem(tag));
                }
            }

        }

    }
}
