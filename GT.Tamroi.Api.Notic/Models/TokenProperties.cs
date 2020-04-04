using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic.Models
{
    public class TokenProperties
    {
        public string ip { get; set; }
        public string createDate { get; set; }
        public string uid { get; set; }

    }

    public class DeTokenProperties
    {
        public string inputIp { get; set; }
        public string inputToken { get; set; }
        public TokenProperties deToken { get; set; }
    }
}