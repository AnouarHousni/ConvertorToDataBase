using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConvertorToDataBase.Exceptions
{
    public class InsertDataException : Exception
    {
        public InsertDataException(string message) : base(message)
        {
        }
    }
}
