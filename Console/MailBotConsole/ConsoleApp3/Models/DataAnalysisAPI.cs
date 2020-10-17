using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    public class DataAnalysisAPI
    {
        public documents[] documents { get; set; }
    }
    public class documents
    {
        public int id { get; set; }
        public string language { get; set; }
        public string text { get; set; }
    }
}
