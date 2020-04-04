using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic.Controllers
{
    public class ReaponseNotic
    {
        public string imei { get; set; }
        public string deviceId { get; set; }
        public int stage { get; set; }
        public string userId { get; set; }

    }
}