using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic.Models.Requests
{
    //public class GetRequestDataRequests
    //{
    //    public List<List<object>> position { get; set; }
    //    public string batteryLevel { get; set; }
    //    public string signalGps { get; set; }
    //    public string deviceId { get; set; }
    //    public int sos { get; set; }
    //    public int Device_Brand_Id { get; set; }
    //    public int Source_Id { get; set; }

    //}


    public class GetRequestDataRequests
    {
        public List<Position> position { get; set; }
        public string imei { get; set; }
        public string userId { get; set; }
        public string batteryLevel { get; set; }
        public string signalGps { get; set; }
        public string deviceId { get; set; }
        public int sos { get; set; }
        public int device_Brand_Id { get; set; }
        public int source_Id { get; set; }
        public int currentFrequency { get; set; }
        public int onlineStatus { get; set; }
        public string accuracy { get; set; }
        public string location_type { get; set; }
    }
    public class Position
    {

        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime gpsDateTime { get; set; }
    }

}