using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel.MetaData
{
    [XmlRoot("option")]
    public class Option
    {
        public Option()
        {

        }
        public Option(String label, int value)
        {
            this.Label = label;
            this.Value = value;
        }
        [XmlAttribute("label")]
        public String Label { get; set; }

        [XmlAttribute("value")]
        public int Value { get; set; }

        public Option Clone()
        {
            Option cloned = new Option();
            cloned.Label = this.Label;
            cloned.Value = this.Value;
            return cloned;

        }
    }
}
