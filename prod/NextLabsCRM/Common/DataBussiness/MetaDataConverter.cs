using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace NextLabs.CRMEnforcer.Common.DataBussiness
{
    public static class MetaDataConverter
    {
        public static List<DataModel.MetaData.Option> CovertDCRMOption2BizMetaDataOption(AttributeMetadata dcrmMetaDataAtrribute)
        {
            List<DataModel.MetaData.Option> bizOptions = new List<DataModel.MetaData.Option>();
            switch (dcrmMetaDataAtrribute.AttributeType)
            {
                case AttributeTypeCode.Boolean:
                    BooleanOptionSetMetadata boolOptionset = ((BooleanAttributeMetadata)dcrmMetaDataAtrribute).OptionSet;
                    DataModel.MetaData.Option bizFalseOption = new DataModel.MetaData.Option();
                    bizFalseOption.Label = boolOptionset.FalseOption.Label.UserLocalizedLabel.Label;
                    bizFalseOption.Value = boolOptionset.FalseOption.Value.Value;
                    bizOptions.Add(bizFalseOption);

                    DataModel.MetaData.Option bizTrueOption = new DataModel.MetaData.Option();
                    bizTrueOption.Label = boolOptionset.TrueOption.Label.UserLocalizedLabel.Label;
                    bizTrueOption.Value = boolOptionset.TrueOption.Value.Value;
                    bizOptions.Add(bizTrueOption);
                    break;
                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    OptionSetMetadata optionset = ((EnumAttributeMetadata)dcrmMetaDataAtrribute).OptionSet;
                    if(optionset != null)
                    {
                        foreach (OptionMetadata option in optionset.Options)
                        {
                            DataModel.MetaData.Option bizOption = new DataModel.MetaData.Option();
                            bizOption.Label = option.Label.UserLocalizedLabel.Label;
                            bizOption.Value = option.Value.Value;
                            bizOptions.Add(bizOption);
                        }
                    }
                    
                    break;
                default:
                    return null;
            }

            return bizOptions;
        }
        public static DataModel.MetaData.Attribute CovertDCRMAttribute2BizMetaDataAttribute(AttributeMetadata dcrmMetaDataAtrribute)
        {
            if (dcrmMetaDataAtrribute == null)
            {
                return null;
            }

            DataModel.MetaData.Attribute bizAttribute = new DataModel.MetaData.Attribute();
            bizAttribute.DataType = dcrmMetaDataAtrribute.AttributeType.Value.ToString();
            bizAttribute.LogicName = dcrmMetaDataAtrribute.LogicalName;

            if (dcrmMetaDataAtrribute.DisplayName.UserLocalizedLabel != null)
            {
                bizAttribute.DisplayName = dcrmMetaDataAtrribute.DisplayName.UserLocalizedLabel.Label;
            }
            else
            {
                bizAttribute.DisplayName = dcrmMetaDataAtrribute.SchemaName;
            }

            bizAttribute.Options = CovertDCRMOption2BizMetaDataOption(dcrmMetaDataAtrribute);

            return bizAttribute;
        }

        private static DataModel.MetaData.Attribute CreateFakeTwoOptionAttribute(string logicName,
            string displayName,
            string trueOptionLabel,
            string falseOptionLabel,
            string description = "")
        {
            DataModel.MetaData.Attribute attr = new DataModel.MetaData.Attribute();
            attr.DataType = "Boolean";
            attr.DisplayName = displayName;
            attr.LogicName = logicName;
            attr.Required = false;
            attr.Description = description;
            attr.Options = new List<DataModel.MetaData.Option>();

            DataModel.MetaData.Option trueOption = new DataModel.MetaData.Option();
            trueOption.Label = trueOptionLabel;
            trueOption.Value = 1;

            DataModel.MetaData.Option falseOption = new DataModel.MetaData.Option();
            falseOption.Label = falseOptionLabel;
            falseOption.Value = 0;

            attr.Options.Add(trueOption);
            attr.Options.Add(falseOption);

            return attr;
        }
        private static DataModel.MetaData.Attribute CreateIsOwnerFakeAttribute()
        {
            return CreateFakeTwoOptionAttribute(Constant.AttributeKeyName.NXL_ISOwner, "Is Owner", "True", "False", "this is a fake attribute for owner allow feature");
        }
        public static DataModel.MetaData.Entity CovertDCRMEntity2BizMetaDataEntity(EntityMetadata dcrmMetaDataEntity)
        {
            if (dcrmMetaDataEntity == null)
            {
                return null;
            }

            DataModel.MetaData.Entity bizEntity = new DataModel.MetaData.Entity();
            List<Common.DataModel.MetaData.Attribute> attributes = new List<Common.DataModel.MetaData.Attribute>();
            bizEntity.Attributes = attributes;

            bizEntity.LogicName = dcrmMetaDataEntity.LogicalName;
            bizEntity.DisplayName = dcrmMetaDataEntity.DisplayName.UserLocalizedLabel.Label;
            bizEntity.PluralName = dcrmMetaDataEntity.DisplayCollectionName.UserLocalizedLabel.Label;

            if (dcrmMetaDataEntity.Attributes == null)
            {
                return bizEntity;
            }

            HashSet<string> parentAttributes = new HashSet<string>();

            foreach (AttributeMetadata currentAttribute in dcrmMetaDataEntity.Attributes)
            {
                if (currentAttribute.AttributeType == AttributeTypeCode.Lookup ||
                   currentAttribute.AttributeType == AttributeTypeCode.Uniqueidentifier ||
                   currentAttribute.AttributeType == AttributeTypeCode.Owner ||
                   currentAttribute.AttributeType == AttributeTypeCode.Customer)
                {
                    parentAttributes.Add(currentAttribute.LogicalName);
                }
            }

            foreach (AttributeMetadata attribute in dcrmMetaDataEntity.Attributes)
            {
                if(attribute.AttributeType == AttributeTypeCode.Virtual)
                {
                    //filter out the virtual attribute
                    continue;
                }

                if(!attribute.IsValidForRead.Value)
                {
                    continue;
                }

                if (attribute.AttributeOf != null)
                {
                    if (parentAttributes.Contains(attribute.AttributeOf))
                    {
                        //skip the child attribute whose parent is lookup,Uniqueidentifier,owner and customer
                        //these attributes cannot be the columns specified to retrieve. otherwise, an exception will be thrown.
                        continue;
                    }
                }

                DataModel.MetaData.Attribute attr = CovertDCRMAttribute2BizMetaDataAttribute(attribute);
                if (attr == null)
                {
                    continue;
                }

                bizEntity.Attributes.Add(attr);
            }

            DataModel.MetaData.Attribute isOwnerFakeAttribute = 
                CreateFakeTwoOptionAttribute(Constant.AttributeKeyName.NXL_ISOwner, "Is Owner", "True", "False", "this is a fake attribute for owner allow feature.");

            DataModel.MetaData.Attribute isSharedFakeAttribute =
                CreateFakeTwoOptionAttribute(Constant.AttributeKeyName.NXL_ISShared, "Is Shared", "True", "False", "this is a fake attribute for shared allow feature.");

            bizEntity.Attributes.Add(isOwnerFakeAttribute);
            bizEntity.Attributes.Add(isSharedFakeAttribute);

            return bizEntity;
        }
    }
}
