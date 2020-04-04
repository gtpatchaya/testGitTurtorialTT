using GT.Tamroi.Api.Notic.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic.Controllers.Helpers
{
    public class Authen
    {
        public bool CheckToken(ref DeTokenProperties input)
        {
            string byPass = ConfigurationSettings.AppSettings["token"].ToString();
            if (input.inputToken == byPass)
                return true;
            else
                return false;
        }

        public TokenProperties Detoken(string token)
        {
            TokenProperties token_ = new TokenProperties();
            return token_;
        }
    }
}