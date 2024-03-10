using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Structure
{
    public class Message<T>
    {
        public T Payload { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public Message()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
        }
    }
}
