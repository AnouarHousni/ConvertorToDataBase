using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Exceptions
{
    public class CreateTableException : Exception
    {
        public CreateTableException(string message) : base(message)
        {
        }
    }
}
