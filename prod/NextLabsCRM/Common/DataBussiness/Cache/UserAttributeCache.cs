using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Cache
{
    class UserAttributeCache : RegularCache<SecureEntity>
    {
        public UserAttributeCache(IStringSerialize serializer) : base(serializer)
        {
        }

        //protected override void BeforeUpdate(string key, SecureEntity bizValue)
        //{
        //    if(string.IsNullOrWhiteSpace(key) || bizValue == null)
        //    {
        //        return;
        //    }
        //    if(key.Equals(Team.EntityLogicalName,StringComparison.OrdinalIgnoreCase) ||
        //        key.Equals(Role.EntityLogicalName, StringComparison.OrdinalIgnoreCase))
        //    {
        //        if(bizValue.Schema == null || bizValue.Schema.Attributes == null)
        //        {
        //            return;
        //        }

        //        foreach(DataModel.MetaData.Attribute attr in bizValue.Schema.Attributes)
        //        {
        //            if(attr.LogicName != null)
        //            {
        //                attr.LogicName = key + "-" + attr.LogicName;
        //            }
        //        }
        //    }
        //}
    }
}
