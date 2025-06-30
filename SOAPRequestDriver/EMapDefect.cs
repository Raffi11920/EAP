using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Drivers.SOAPRequestDriver
{
    class EMapDefect
    {
        public string DefectCode { get; set; }
        public string row { get; set; }
        public string column { get; set; }

        EMapDefect()
        {

        }

        public EMapDefect(string defect, string row, string column)
        {
            this.DefectCode = defect;
            this.row = row;
            this.column = column;
        }
    }
}
