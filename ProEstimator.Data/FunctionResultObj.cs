using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData
{
    public class FunctionResultObj<T> : FunctionResult
    {

        public T Object { get; protected set; }

        public FunctionResultObj(T newObject)
            : base()
        {
            Object = newObject;
        }

        public FunctionResultObj(string errorMessage)
            : base(errorMessage)
        {
            
        }

    }
}
