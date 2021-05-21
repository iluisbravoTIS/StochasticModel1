using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1.Models.Structs
{
    struct TimePrice_param
    {
        //t,s,nCUT
        public int nCUT { get; set; }
        public T_param T { get; set; }
        public Scen_set SCEN { get; set; }
        public double TIMEPRICE { get; set; }
        public double DUAL { get; set; }
    }
}
