using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic
{
    public class TrackingResponse
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public int battery { get; set; }
        public string inout_status { get; set; }
        public string  current_datetime { get; set; }
        public string thai_address { get; set; }
        public string english_address { get; set; }
        public string online_status { get; set;  } 
    }
}