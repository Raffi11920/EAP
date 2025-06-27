using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Base.BridgeMessage
{
    public class BasicData
    {
        public Type DataType { get; set; }

        /// <summary>
        /// Case insensitive string
        /// </summary>
        public string Name { get; set; }
        public object Value { get; set; }

        public BasicData()
        {

        }

        public BasicData(string name, object value, Type dataType)
        {
            Name = name;
            Value = value;
            DataType = dataType;
        }
    }
}
