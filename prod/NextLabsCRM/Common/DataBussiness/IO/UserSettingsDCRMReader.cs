using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataModel;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    class UserSettingsDCRMReader:IUserSettingsReader
    {
        public UserSettingsDCRMReader(IOrganizationService service)
        {
            m_service = service;
        }
        private IOrganizationService m_service;
        public List<UserSetting> ReadAll()
        {
            if (m_service == null)
            {
                return null;
            }

            QueryExpression query = CreateBasicQueryExpression();

            EntityCollection entityCollection = EntityDataHelp.RetrieveMultiple(m_service,query);
            DataCollection<Entity> entities = entityCollection.Entities;
            if (entities == null || entities.Count == 0)
            {
                return null;
            }

            List<DataModel.UserSetting> records = new List<DataModel.UserSetting>();
            foreach (Entity entity in entities)
            {
                DataModel.UserSetting userSetting = ConvertDCRMEntity2UserSetting(entity);
                if (userSetting == null)
                {
                    continue;
                }
                records.Add(userSetting);
            }

            return records;
        }

        private DataModel.UserSetting ConvertDCRMEntity2UserSetting(Entity entity)
        {
            DataModel.UserSetting userSetting = new DataModel.UserSetting();
            RecordFactory<DataModel.UserSetting>.Create(entity, ref userSetting);

            return userSetting;
        }
        private QueryExpression CreateQueryExpression(List<string> entityLogicNames)
        {
            QueryExpression query = CreateBasicQueryExpression();
            ConditionExpression nameCondition = new ConditionExpression(
                UserSettingField.ColName,
                ConditionOperator.In,
                entityLogicNames.ToArray());
            query.Criteria.AddCondition(nameCondition);
            return query;
        }

        /// <summary>
        /// Create a basic query expression for all the enabled records in nxl_settings entity
        /// </summary>
        /// <returns></returns>
        private QueryExpression CreateBasicQueryExpression()
        {
            string[] columns = { UserSettingField.ColName, UserSettingField.ColDataType,
                UserSettingField.ColEnabled,UserSettingField.ColContent, UserSettingField.ColResolved1,UserSettingField.ColResolved2};
            QueryExpression query = new QueryExpression
            {
                
                EntityName = UserSettingField.SchemaName,                     //"nxl_settings"
                ColumnSet = new ColumnSet(columns), //"content"
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = UserSettingField.ColEnabled,     //"enabled"
                            Operator = ConditionOperator.Equal,
                            Values = {true}
                        }
                    }
                }
            };

            return query;
        }
        private QueryExpression CreateQueryExpression(string entityLogicName)
        {
            QueryExpression query = CreateBasicQueryExpression();
            ConditionExpression nameCondition = new ConditionExpression
            {
                AttributeName = UserSettingField.ColName,   //"name"
                Operator = ConditionOperator.In,
                Values = { entityLogicName }
            };

            query.Criteria.Conditions.Add(nameCondition);

            return query;
        }

        
    }
}
