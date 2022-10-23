using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    [AttributeUsage(AttributeTargets.Class)]
    class SchemaAttribute : Attribute
    {
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    class SchemaColumnAttribute : Attribute
    {
        public string Name { get; set; }

        public Type Type { get; set; }
        public Boolean Enabled { get; set; }

        public Boolean Required { get; set; }
    }
}
