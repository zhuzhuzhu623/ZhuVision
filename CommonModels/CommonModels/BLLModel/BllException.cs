using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.BllModel
{
    [Serializable]
    public class BllException : ApplicationException
    {
        public BllException()
        {
        }
        public BllException(string message) : base(message)
        {
        }
        public BllException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
