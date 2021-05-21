using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1.Models.Structs
{
    class Balance2Price_param
    {
        public int nCUT { get; set; }
        public Prod_set PROD { get; set; }
        public Scen_set SCEN { get; set; }
        public double DUAL { get; set; }
        public double BALANCE2PRICE { get; set; }
    }
}
