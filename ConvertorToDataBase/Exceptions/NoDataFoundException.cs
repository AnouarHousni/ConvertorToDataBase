using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Exceptions
{
    public class NoDataFoundException : Exception
    {
        public NoDataFoundException(string sheetName) : base($"No data found in sheet: {sheetName}")
        { }
    }
}
