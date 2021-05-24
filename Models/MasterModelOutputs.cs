using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1.Models
{
    class MasterModelOutputs
    {
        public double expectedProfit { get; set; }
        public GRBVar[] make1 { get; set; }
        public GRBVar[] sell1 { get; set; }
        public GRBVar[] inv1 { get; set; }
        public GRBModel gModel { get; set; }
    }
}
