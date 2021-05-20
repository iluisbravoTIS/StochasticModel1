using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1
{
    public class IndexesVar
    {
        public int getIx3(int a, int b, int c, int nA, int nB, int nC) {        
            int result = a * nB * nC + b * nC + c;
            return result;
        }

        public int getIx2(int a, int b, int nA, int nB)
        {
            int result = a * nB + b;
            return result;
        }
    }
}
