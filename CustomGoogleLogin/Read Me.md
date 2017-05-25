for CUSTOM google login in MVC without Identity
----------------------------------------------

1) Install Required Pakages
>Install-Package Microsoft.Owin.Security.Google

For GetOwinContext() method
>Install-Package Microsoft.Owin.Host.SystemWeb

For CookieAuthenticationDefaults.AuthenticationType
>Install-Package Microsoft.Owin.Security.Cookies

2) Configuration

i)  Add Startup.cs in route folder of application if not already exist Then add Startup.Auth.cs class in App_Data folder

ii) Add following code in Startup.Auth.cs file

	public partial class Startup { 
		public void ConfigureAuth(IAppBuilder app) {
			 app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
			 app.UseCookieAuthentication(new CookieAuthenticationOptions
			 {
				 LoginPath = new PathString("/Account/Index"),
				 SlidingExpiration = true 
			 });
			 app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
			 {
				 ClientId = "857589842598-d29av6ftecn7bnnmhgplecafpt6tnfo0.apps.googleusercontent.com",
				 ClientSecret = "WQVQK_PDMesI_F-00e0U-G_S" ,
				 CallbackPath = new PathString("/GoogleLoginCallback")
			 });
		 }
	}

iii) Update Startup.cs file as following

	public partial class Startup{
		public void Configuration(IAppBuilder app) 
		{ 
		ConfigureAuth(app); // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
		}
	}

Note:- Both files must be in same namespace


3) Changes in Account Controller
-----------------------------

i) Default Page for login:

public ActionResult Index() { return View(); }


ii) Create View as following

 @{
 	ViewBag.Title = "Index";
 }

 <h2>Login</h2>
 <div style="margin-top:20px">

 </div>
 <div class="row" style="margin-top:20px;">
 	<div class="col-md-5"> 
 		<p>
 			<a class="btn btn-default btn-block" href="@Url.Action("SignIn", new { type = "Google" })"><i class="fa fa-google"></i>&nbsp;&nbsp;Login using Google</a>
 		</p>
 	</div>

 </div>

iii) Add Sign in method

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


iv) Then Create method For GoogleSignIn Call Back

 [AllowAnonymous]
 public ActionResult GoogleLoginCallback()
 {
 	var claimsPrincipal = HttpContext.User.Identity as ClaimsIdentity;

 	var loginInfo = GoogleLoginViewModel.GetLoginInfo(claimsPrincipal);
 	if (loginInfo == null)
 	{
 		return RedirectToAction("Index");
 	}
     

 	MovieEntities db = new MovieEntities();
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

4) ViewModel and Database

i) GoogleLogin ViewModel Class should be like this

public class GoogleLoginViewModel {
 public string emailaddress { get; set; }
 public string name { get; set; }
 public string givenname { get; set; }
 public string surname { get; set; }
 public string nameidentifier { get; set; }

 internal static GoogleLoginViewModel GetLoginInfo(ClaimsIdentity identity)
 {
 	if(identity.Claims.Count() == 0 || identity.Claims.FirstOrDefault(x=> x.Type == ClaimTypes.Email) == null)
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

