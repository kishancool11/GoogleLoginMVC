using CustomGoogleLogin.Models;
using CustomGoogleLogin.ViewModel;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace CustomGoogleLogin.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public void SignIn(string ReturnUrl = "/", string type = "")
        {
            if (!Request.IsAuthenticated)
            {
                if (type == "Google")
                {
                    HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "Account/GoogleLoginCallback" }, "Google");
                }
            }
        }
	public ActionResult SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Redirect("~/");
        }
	
        [AllowAnonymous]
        public ActionResult GoogleLoginCallback()
        {
            var claimsPrincipal = HttpContext.User.Identity as ClaimsIdentity;

            var loginInfo = GoogleLoginViewModel.GetLoginInfo(claimsPrincipal);
            if (loginInfo == null)
            {
                return RedirectToAction("Index");
            }


            WebEntities db = new WebEntities(); //DbContext
            var user = db.UserAccounts.FirstOrDefault(x => x.Email == loginInfo.emailaddress);

            if (user == null)
            {
                user = new UserAccount
                {
                    Email = loginInfo.emailaddress,
                    GivenName = loginInfo.givenname,
                    Identifier = loginInfo.nameidentifier,
                    Name = loginInfo.name,
                    SurName = loginInfo.surname,
                    IsActive = true
                };
                db.UserAccounts.Add(user);
                db.SaveChanges();
            }

            var ident = new ClaimsIdentity(
                    new[] { 
									// adding following 2 claim just for supporting default antiforgery provider
									new Claim(ClaimTypes.NameIdentifier, user.Email),
                                    new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"),

                                    new Claim(ClaimTypes.Name, user.Name),
                                    new Claim(ClaimTypes.Email, user.Email),
									// optionally you could add roles if any
									new Claim(ClaimTypes.Role, "User")
                    },
                    CookieAuthenticationDefaults.AuthenticationType);


            HttpContext.GetOwinContext().Authentication.SignIn(
                        new AuthenticationProperties { IsPersistent = false }, ident);
            return Redirect("~/");

        }
    }
}
