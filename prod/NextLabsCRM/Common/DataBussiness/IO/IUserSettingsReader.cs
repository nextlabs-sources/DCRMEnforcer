using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    interface IUserSettingsReader
    {
        List<Common.DataModel.UserSetting> ReadAll();
    }
}
