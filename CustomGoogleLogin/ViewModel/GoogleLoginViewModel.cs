using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace CustomGoogleLogin.ViewModel
{
    public class GoogleLoginViewModel
    {
        public string emailaddress { get; set; }
        public string name { get; set; }
        public string givenname { get; set; }
        public string surname { get; set; }
        public string nameidentifier { get; set; }

        internal static GoogleLoginViewModel GetLoginInfo(ClaimsIdentity identity)
        {
            if (identity.Claims.Count() == 0 || identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email) == null)
            {
                return null;
            }

            return new GoogleLoginViewModel
            {
                emailaddress = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value,
                name = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value,
                givenname = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName).Value,
                surname = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname).Value,
                nameidentifier = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value,
            };
        }
    }
}