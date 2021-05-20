using ModelStochastic1.Models.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1.Models
{
    struct InputData
    {
        public List<T_param> TList { get; set; }
        public List<Prod_set> ProdList { get; set; }
        public List<Scen_set> ScenList { get; set; }
        public List<Avail_param> AvailList { get; set; }
        public List<Rate_param> RateList { get; set; }
        public List<Inv0_param> Inv0List { get; set; }
        public List<ProdCost_param> ProdCostList { get; set; }
        public List<InvCost_param> InvCostList { get; set; }
        public List<Revenue_param> RevenueList { get; set; }
        public List<Market_param> MarketList { get; set; }
        public List<Prob_param> ProbList { get; set; }

    }
}
