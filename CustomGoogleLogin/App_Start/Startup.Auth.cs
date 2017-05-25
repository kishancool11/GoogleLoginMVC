using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin; 

namespace CustomGoogleLogin
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                LoginPath = new PathString("/Account/Index"),
                SlidingExpiration = true
            });
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "857589842598-d29av6ftecn7bnnmhgplecafpt6tnfo0.apps.googleusercontent.com",
                ClientSecret = "WQVQK_PDMesI_F-00e0U-G_S",
                CallbackPath = new PathString("/GoogleLoginCallback")
            });
        }
    }
}