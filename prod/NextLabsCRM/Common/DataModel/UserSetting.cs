using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    public enum RecordType
    {
        Entity = 280240000,
        GeneralSetting = 280240004,
        TestPC = 280240006,
        UserAttribute = 280240001,
        N1Relationship = 280240002,
        NNRelationship = 280240003,
        LogLevel = 280240005,
        IOCommand = 280240007,
        ImportPM = 280240009,
        TestWC = 280240010,
    }

    public enum ServerType {
        CC,
        JPC,
    }

    //for login cloudAz web console
    public enum WCproxyResult
    {
        HttpFaild_Unknown,//This maybe network transmission problem
        OK,//Call Success
        Invalid_Usr_Pwd,
        Invalid_Host,
        Failed//Call Faild
    }

    class UserSettingField
    {
        public const string SchemaName = "nxl_setting";
        public const string ColName = "nxl_name";
        public const string ColDataType = "nxl_datatype";
        public const string ColEnabled = "nxl_enabled";
        public const string ColContent = "nxl_content";
        public const string ColResolved1 = "nxl_resolved1";
        public const string ColResolved2 = "nxl_resolved2";

        public const string ColId = "nxl_settingid";


    }

    class UniqueRecordName
    {
        public const string LogSettingsName = "NXLLogSettings";
        public const string GeneralSettingsName = "NXLGeneralSettings";
        public const string TestPCName = "NXLTestPC";
        public const string ImportPMName = "importPM";
    }

    [Schema(Name = "nxl_setting")]
    class UserSetting
    {
        [SchemaColumn (Name = "nxl_name", Required =true)]
        public string Name { get; set; }

        [SchemaColumn(Name = "nxl_datatype", Required =true)]
        public RecordType DataType { get; set; }

        [SchemaColumn(Name = "nxl_enabled", Required =true)]
        public Boolean Enabled { get; set; }

        [SchemaColumn(Name = "nxl_content", Required =true)]
        public string Content { get; set; }

        [SchemaColumn(Name = "nxl_resolved1", Required =false)]
        public string Resolved1 { get; set; }

        [SchemaColumn(Name = "nxl_resolved2", Required =false)]
        public string Resolved2 { get; set; }
    }

    static class RecordFactory<T>
    {
        static public void Create(Entity entity, ref T schema)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }

            var info = typeof(T);
            var schemaAttr = (SchemaAttribute)Attribute.GetCustomAttribute(info, typeof(SchemaAttribute));

            if (!entity.LogicalName.Equals(schemaAttr.Name,StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception(string.Format("Cannot covert Entity {0} to {1}", entity.LogicalName, schemaAttr.Name));
            }

            var propertiesInfoes = info.GetProperties();
            foreach (var propertyInfo in propertiesInfoes)
            {
                var columnSchemaAttr = (SchemaColumnAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(SchemaColumnAttribute));

                object attr = null;
                try
                {
                    attr = entity.Attributes[columnSchemaAttr.Name];
                }
                catch(Exception e)
                {
                    attr = null;
                }
                
                if(attr == null && columnSchemaAttr.Required)
                {
                    throw new Exception("{attr} is requred, it's value must be assigned");
                }

                if( columnSchemaAttr.Required &&
                    attr is string && 
                    string.IsNullOrWhiteSpace(attr as string))
                {
                    throw new Exception("{attr} is requred, it's value must not be empty");
                }

                if (attr is OptionSetValue)
                    attr = ((OptionSetValue)attr).Value;
                propertyInfo.SetValue(schema,attr);
            }
        }
    }
}
