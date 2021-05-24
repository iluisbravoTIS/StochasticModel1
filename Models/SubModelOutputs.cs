using Gurobi;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1.Models.Structs
{
    class SubModelOutputs
    {
        public double stage2Profit { get; set; }
        public GRBVar[] make { get; set; }
        public GRBVar[] sell { get; set; }
        public GRBVar[] inv { get; set; }
        public GRBModel gModel { get; set; }

    }
}
