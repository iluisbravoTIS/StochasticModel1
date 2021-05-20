using ModelStochastic1.Models.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1.Models
{
    struct MasterModelParameters
    {
        public List<TimePrice_param> timePriceList { set; get; }
        public List<Balance2Price_param> balance2PriceList { get; set; }
        public List<SellLimPrice_param> sellLimPriceList { get; set; }
        public double Min_Stage2_Profit { get; set; }

    }
}
