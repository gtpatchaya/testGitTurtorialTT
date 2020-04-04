using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic.Models.Response
{
    public class GetResponsDataResponse
    {
        public string status { get; set; }
        public int statusCode { get; set; }
        public Data data { get; set; }
    }
    public class Data
    {
        public string AddressTh { get; set; }
        public string AddressEn { get; set; }

    }
}