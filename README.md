# Guide for custom google login in MVC without default Identity
-------------------------------------------------------
Create google app key and seceret, [Follow the instruction here](https://developers.google.com/identity/sign-in/web/devconsole-project)
set callback url to [http://localhost:3002/GoogleLoginCallback](#)

Create one MVC application with no Authentication Selected.


# 1)  Install Required Pakages
>Install-Package Microsoft.Owin.Security.Google

For GetOwinContext() method
>Install-Package Microsoft.Owin.Host.SystemWeb

For CookieAuthenticationDefaults.AuthenticationType
>Install-Package Microsoft.Owin.Security.Cookies

# 2) Configuration

i)  Add Owin Startup Class file "Startup.cs" in route folder of application, if not already exist.
Then add a class file in App_Data folder with name Startup.Auth.cs 

ii) Add following code in Startup.Auth.cs file

	using Microsoft.Owin;
	using Microsoft.Owin.Security;
	using Microsoft.Owin.Security.Cookies;
	using Microsoft.Owin.Security.Google;
	using Owin; 

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
				 ClientId = "Your Client ID",
				 ClientSecret = "Your Secret Key" ,
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

Note:- Both files must be in same namespace and partial class

# 3) ViewModel and Database

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


ii) Create one database and Add Following Table in it

	CREATE TABLE [dbo].[UserAccount](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Email] [nvarchar](max) NOT NULL,
		[GivenName] [nvarchar](max) NULL,
		[Name] [nvarchar](max) NULL,
		[SurName] [nvarchar](max) NULL,
		[Identifier] [nvarchar](max) NOT NULL,
		[IsActive] [bit] NOT NULL,
	 CONSTRAINT [PK_UserAccount] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	GO
iii) Create One  Edmx File and Add above table in this edmx file. You can also use code first approach if you wish

# 4) Changes in Account Controller 

Create One AccountController Classs and Add Following 

	using Microsoft.Owin.Security.Cookies; 
	using System.Security.Claims;
	using Microsoft.Owin.Security;

Then you can add following  

i) Default Page for login:

	public ActionResult Index() {
		return View();
	}


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

# 5) Validate
i) Add Following line in you layout page

    @if (User.Identity.IsAuthenticated)
    {
        <ul class="nav navbar-nav navbar-right"> 
            <li><a href="#">@User.Identity.Name</a></li>  
        </ul>
    }
				
ii) Create one controller as following to validate login
 
    [Authorize]
    public class UsersController : Controller
    {
    // GET: Users
    public ActionResult Index()
    {
        WebEntities db = new WebEntities();
        var list = db.UserAccounts.ToList();
        return View(list);
    }
    } 
