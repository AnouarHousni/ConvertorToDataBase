using ConvertorToDataBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Models
{
    public class TableColumn
    {
        public string? Name { get; set; }
        public string? DataType { get; set; }
        public int Length { get; set; }
        public ColumnOption ColumnOption { get; set; }
        public string? DateFormat { get; set; }
        public string? DefaultValue { get; set; }
    }
}
