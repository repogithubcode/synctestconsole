using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData
{
    public class FunctionResultInt : FunctionResult
    {
        public int Value { get; private set; }

        public FunctionResultInt(int value)
            : base()
        {
            Value = value;
        }

        public FunctionResultInt(string errorMessage)
            : base(errorMessage)
        {

        }
    }
}
