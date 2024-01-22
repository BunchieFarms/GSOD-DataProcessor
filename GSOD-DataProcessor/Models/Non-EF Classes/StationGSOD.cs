using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSOD_DataProcessor.Models
{
    public class StationGSOD
    {
        public string Station { get; set; }
        public DateTime Date { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Name { get; set; }
        public float Temp { get; set; }
        public float Prcp { get; set; }
    }
}
