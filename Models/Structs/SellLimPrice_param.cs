using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1.Models.Structs
{
    class SellLimPrice_param
    {
        //p, t, s, nCUT
        public int nCUT { get; set; }
        public T_param T { get; set; }
        public Scen_set SCEN { get; set; }
        public Prod_set PROD { get; set; }
        public double URC { get; set; }
        public double SELLLIMPRICE { get; set; }
    }
}
